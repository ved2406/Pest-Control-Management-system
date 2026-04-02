using PestControl.Api.Repositories.Interfaces;
using PestControl.Api.Repositories.Sql;
using PestControl.Api.Services;

// Program.cs is the entry point of the entire application.
// It sets up Dependency Injection (DI) — a system where .NET automatically
// creates and provides objects (like repositories) to the classes that need them.
// Instead of manually writing "new SqlCustomerRepository()" everywhere,
// we register it once here and .NET injects it wherever it's needed.

var builder = WebApplication.CreateBuilder(args);

// The connection string tells ADO.NET how to connect to our SQL Server database.
// Server=DESKTOP-JUVCBLB is the machine name where SQL Server is installed.
// TrustServerCertificate=True skips SSL certificate validation (fine for local dev).
string connectionString = "Server=DESKTOP-JUVCBLB;Database=PestControlDB;Trusted_Connection=True;TrustServerCertificate=True;";

// Register all 6 SQL repositories as Singletons.
// Singleton means only ONE instance is created for the lifetime of the app —
// every controller that asks for ICustomerRepository gets the same object.
// The interface (e.g. ICustomerRepository) is the contract;
// the implementation (SqlCustomerRepository) is what actually runs the SQL queries.
builder.Services.AddSingleton<ICustomerRepository>(new SqlCustomerRepository(connectionString));
builder.Services.AddSingleton<IPestTypeRepository>(new SqlPestTypeRepository(connectionString));
builder.Services.AddSingleton<IBookingRepository>(new SqlBookingRepository(connectionString));
builder.Services.AddSingleton<ITechnicianRepository>(new SqlTechnicianRepository(connectionString));
builder.Services.AddSingleton<ITreatmentRepository>(new SqlTreatmentRepository(connectionString));
builder.Services.AddSingleton<IInspectionReportRepository>(new SqlInspectionReportRepository(connectionString));

// Register the SearchService — it searches across all 6 data types at once.
// .NET will automatically inject the 6 repositories above into its constructor.
builder.Services.AddSingleton<SearchService>();

// Register the AI Agent as a Singleton.
// We use a factory lambda (sp => ...) because PestControlAgent needs the API key
// passed manually — it can't be auto-injected since it's just a string.
// sp.GetRequiredService<T>() retrieves the already-registered repository from DI.
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

// Register all controllers (CustomersController, BookingsController, etc.)
// .NET scans for classes with [ApiController] and wires them up automatically.
builder.Services.AddControllers();

var app = builder.Build();

// Serve index.html automatically when someone visits http://localhost:5073/
// This is how our frontend (wwwroot/index.html) is delivered to the browser.
app.UseDefaultFiles();

// Serve all static files from the wwwroot folder (HTML, CSS, JS, images)
app.UseStaticFiles();

// Map all controller routes — e.g. [Route("api/customers")] -> CustomersController
app.MapControllers();

// Start the web server and begin listening for requests
app.Run();