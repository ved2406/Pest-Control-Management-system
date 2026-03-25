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

builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
