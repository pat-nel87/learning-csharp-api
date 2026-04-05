using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ServiceHealthApi.Data;
using ServiceHealthApi.Services;

// ============================================================================
// Program.cs — The entry point for the ASP.NET Core application.
//
// This file uses "top-level statements" (no Main method needed).
// Everything here runs when you do `dotnet run`.
//
// The two main phases are:
// 1. CONFIGURE SERVICES — register dependencies in the DI container
// 2. CONFIGURE MIDDLEWARE — set up the HTTP request pipeline
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ----- PHASE 1: CONFIGURE SERVICES (Dependency Injection) -----

// Register controllers — ASP.NET scans for classes that inherit ControllerBase
// and wires up routing based on their [Route] and [Http*] attributes.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings ("Healthy") instead of numbers (1) in JSON responses.
        // This makes the API output much more readable.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Register the DbContext — Scoped lifetime (one instance per HTTP request).
// UseSqlite tells EF Core to use a local SQLite database file.
// No server setup needed — the file is created automatically.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=servicehealthapi.db"));

// Register our service monitor — THIS IS DEPENDENCY INJECTION:
// Controllers receive IServiceMonitor without knowing the concrete class.
// Want to swap in a mock for testing? Change this one line.
// Want a different implementation for production? Change this one line.
// The controllers never change. That's the power of DI.
builder.Services.AddScoped<IServiceMonitor, ServiceMonitor>();

// Configure Swagger/OpenAPI for interactive API documentation.
// After starting the app, visit /swagger to see it in action.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Service Health Tracker",
        Version = "v1",
        Description = "A learning API for tracking microservice health status"
    });

    // Include XML comments in Swagger UI so it shows our /// <summary> docs
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ----- SEED THE DATABASE -----
// Create a scope to get a DbContext instance and seed initial data.
// We do this at startup so the database has data on first run.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.InitializeAsync(context);
}

// ----- PHASE 2: CONFIGURE MIDDLEWARE (HTTP Pipeline) -----
// Middleware runs in order for every request. Order matters!

// Global exception handler — catches unhandled exceptions and returns
// a structured ProblemDetails JSON response instead of a stack trace.
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "An unexpected error occurred",
            status = 500
        });
    });
});

// Enable Swagger in all environments (since this is a learning project)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Service Health Tracker v1");
    // Make Swagger UI the default page when you navigate to the root URL
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// This partial class declaration makes Program accessible to WebApplicationFactory in tests.
// Without it, the test project can't reference Program because top-level statements
// generate an implicit Program class that's internal by default.
public partial class Program { }
