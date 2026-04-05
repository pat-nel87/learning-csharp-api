# Service Health Tracker API

A learning project that combines a C# ASP.NET Core Web API with Playwright browser testing. The domain — tracking microservice health — is intentionally familiar so the focus stays on learning C# and Playwright patterns.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (8.0 or later)
- PowerShell or bash (for Playwright browser install)

## Getting Started

```bash
# Restore NuGet packages
dotnet restore

# Run the API (database is created and seeded automatically)
dotnet run --project ServiceHealthApi

# Open Swagger UI in your browser
# Navigate to http://localhost:5281
```

## Running Tests

```bash
# Build the test project
dotnet build ServiceHealthApi.Tests

# Install Playwright browsers (first time only)
pwsh ServiceHealthApi.Tests/bin/Debug/net10.0/playwright.ps1 install

# Run all tests
dotnet test ServiceHealthApi.Tests
```

## Project Tour

| File | What It Demonstrates |
|------|---------------------|
| `Models/ServiceEntry.cs` | Entity model with enums, nullable types, and XML doc comments |
| `Models/HealthStatus.cs` | DTO pattern — separating API responses from database entities |
| `Data/AppDbContext.cs` | EF Core DbContext — the bridge between C# and SQLite |
| `Data/SeedData.cs` | Programmatic database seeding with sample data |
| `Services/IServiceMonitor.cs` | Interface design for Dependency Injection |
| `Services/ServiceMonitor.cs` | async/await patterns with simulated network calls |
| `Controllers/ServicesController.cs` | Full CRUD with proper HTTP status codes |
| `Controllers/HealthCheckController.cs` | DI with interfaces, POST with side effects |
| `Program.cs` | DI registration, Swagger config, middleware pipeline |
| `Tests/Fixtures/ApiFixture.cs` | WebApplicationFactory + Playwright browser setup |
| `Tests/ApiTests/` | HttpClient-based API testing |
| `Tests/UiTests/SwaggerUiTests.cs` | Browser automation with Playwright locators and assertions |

## Learning Exercises

Try these modifications to deepen your understanding:

1. **Add a PATCH endpoint** for partial updates (only update fields that are provided)
2. **Add pagination** to the GET all endpoint (`?page=1&pageSize=10`)
3. **Write an end-to-end test** that creates a service, health-checks it, then verifies the status changed
4. **Add a simple dashboard page** using Razor Pages and write Playwright tests for it
5. **Swap SQLite for PostgreSQL** and observe that only `Program.cs` changes — that's DI in action
