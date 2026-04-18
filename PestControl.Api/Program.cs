using PestControl.Api.Repositories.Interfaces;
using PestControl.Api.Repositories.Sql;
using PestControl.Api.Services;

// App startup — register all services and configure dependency injection here
// This tells .NET what objects to create and when to create them

var builder = WebApplication.CreateBuilder(args);

// Connection string tells us where the SQL Server database is located
string connectionString = "Server=DESKTOP-JUVCBLB;Database=PestControlDB;Trusted_Connection=True;TrustServerCertificate=True;";

// Register all data repositories as singletons — one instance shared across the whole app
// So every controller gets the same repository object
builder.Services.AddSingleton<ICustomerRepository>(new SqlCustomerRepository(connectionString));
builder.Services.AddSingleton<IPestTypeRepository>(new SqlPestTypeRepository(connectionString));
builder.Services.AddSingleton<IBookingRepository>(new SqlBookingRepository(connectionString));
builder.Services.AddSingleton<ITechnicianRepository>(new SqlTechnicianRepository(connectionString));
builder.Services.AddSingleton<ITreatmentRepository>(new SqlTreatmentRepository(connectionString));
builder.Services.AddSingleton<IInspectionReportRepository>(new SqlInspectionReportRepository(connectionString));

// Search service searches across all repositories at once
builder.Services.AddSingleton<SearchService>();

// AI agent with API key — needs custom setup because the API key is passed in manually
builder.Services.AddSingleton<PestControlAgent>(sp =>
    new PestControlAgent(
        sp.GetRequiredService<ICustomerRepository>(),
        sp.GetRequiredService<IPestTypeRepository>(),
        sp.GetRequiredService<IBookingRepository>(),
        sp.GetRequiredService<ITechnicianRepository>(),
        sp.GetRequiredService<ITreatmentRepository>(),
        sp.GetRequiredService<IInspectionReportRepository>(),
        Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "your-api-key-here"
    ));

// Register all controller classes so their endpoints work
builder.Services.AddControllers();

var app = builder.Build();

// When someone visits the app, serve index.html by default
app.UseDefaultFiles();

// Serve static files — CSS, JS, images, everything from wwwroot folder
app.UseStaticFiles();

// Wire up all the API routes from our controllers
app.MapControllers();

// Start listening for requests
app.Run();