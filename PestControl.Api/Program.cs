using PestControl.Api.Repositories.Interfaces;
using PestControl.Api.Repositories.Sql;
using PestControl.Api.Services;

var builder = WebApplication.CreateBuilder(args);

string connectionString = "Server=DESKTOP-JUVCBLB;Database=PestControlDB;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddSingleton<ICustomerRepository>(new SqlCustomerRepository(connectionString));
builder.Services.AddSingleton<IPestTypeRepository>(new SqlPestTypeRepository(connectionString));
builder.Services.AddSingleton<IBookingRepository>(new SqlBookingRepository(connectionString));
builder.Services.AddSingleton<ITechnicianRepository>(new SqlTechnicianRepository(connectionString));
builder.Services.AddSingleton<ITreatmentRepository>(new SqlTreatmentRepository(connectionString));
builder.Services.AddSingleton<IInspectionReportRepository>(new SqlInspectionReportRepository(connectionString));

builder.Services.AddSingleton<SearchService>();

builder.Services.AddSingleton<PestControlAgent>(sp =>
    new PestControlAgent(
        sp.GetRequiredService<ICustomerRepository>(),
        sp.GetRequiredService<IPestTypeRepository>(),
        sp.GetRequiredService<IBookingRepository>(),
        sp.GetRequiredService<ITechnicianRepository>(),
        sp.GetRequiredService<ITreatmentRepository>(),
        sp.GetRequiredService<IInspectionReportRepository>(),
        builder.Configuration["AnthropicApiKey"]
            ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
            ?? throw new InvalidOperationException("AnthropicApiKey is not configured")
    ));

builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
