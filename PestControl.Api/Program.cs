using PestControl.Api.Repositories.Interfaces;
using PestControl.Api.Repositories.Sql;
using PestControl.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// SQL Server connection string — update Server name to match your SSMS instance
string connectionString = "Server=DESKTOP-JUVCBLB;Database=PestControlDB;Trusted_Connection=True;TrustServerCertificate=True;";

// Register SQL repositories (reads/writes to SQL Server via SSMS)
builder.Services.AddSingleton<ICustomerRepository>(new SqlCustomerRepository(connectionString));
builder.Services.AddSingleton<IPestTypeRepository>(new SqlPestTypeRepository(connectionString));
builder.Services.AddSingleton<IBookingRepository>(new SqlBookingRepository(connectionString));
builder.Services.AddSingleton<ITechnicianRepository>(new SqlTechnicianRepository(connectionString));
builder.Services.AddSingleton<ITreatmentRepository>(new SqlTreatmentRepository(connectionString));
builder.Services.AddSingleton<IInspectionReportRepository>(new SqlInspectionReportRepository(connectionString));

// Register services
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<PestControlAgent>(sp =>
    new PestControlAgent(
        sp.GetRequiredService<ICustomerRepository>(),
        sp.GetRequiredService<IPestTypeRepository>(),
        sp.GetRequiredService<IBookingRepository>(),
        sp.GetRequiredService<ITechnicianRepository>(),
        sp.GetRequiredService<ITreatmentRepository>(),
        sp.GetRequiredService<IInspectionReportRepository>(),
        "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"
    ));

builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
