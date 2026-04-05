using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using ServiceHealthApi.Data;

namespace ServiceHealthApi.Tests.Fixtures;

/// <summary>
/// Shared test fixture that sets up:
/// 1. An in-process test server (WebApplicationFactory) — no need to manually start the API
/// 2. A Playwright browser instance for UI tests
/// 3. A fresh in-memory database for each test run
///
/// WebApplicationFactory is the key class here — it hosts your ASP.NET Core app
/// in the test process, so tests can hit real endpoints without network overhead.
/// </summary>
[SetUpFixture]
public class ApiFixture
{
    // Static so all tests in the assembly share the same server and browser
    public static WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public static HttpClient HttpClient { get; private set; } = null!;
    public static string BaseUrl { get; private set; } = null!;

    // Playwright objects for browser-based UI tests
    public static IPlaywright PlaywrightInstance { get; private set; } = null!;
    public static IBrowser Browser { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        // Create the test server with a custom configuration.
        // WithWebHostBuilder lets us override services — here we swap SQLite file
        // for an in-memory database so tests don't affect the real database.
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add an in-memory database for testing — fast and isolated
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));
                });
            });

        // CreateClient starts the test server and gives us an HttpClient pointed at it
        HttpClient = Factory.CreateClient();
        BaseUrl = HttpClient.BaseAddress!.ToString().TrimEnd('/');

        // Set up Playwright for browser-based tests
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            // Set to true to watch tests run in a visible browser window (great for debugging)
            Headless = true
        });
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        // Clean up in reverse order of creation
        if (Browser != null) await Browser.CloseAsync();
        PlaywrightInstance?.Dispose();
        HttpClient?.Dispose();
        if (Factory != null) await Factory.DisposeAsync();
    }
}
