using ServiceHealthApi.Models;

namespace ServiceHealthApi.Data;

/// <summary>
/// Seeds the database with sample services on first run.
/// This is a static helper class — it doesn't need an instance, just call SeedData.InitializeAsync().
///
/// We seed data programmatically (instead of in OnModelCreating) so we can use async
/// and have more control over when seeding happens.
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // EnsureCreated creates the database and tables if they don't exist.
        // In a real project you'd use migrations instead, but this is simpler for learning.
        await context.Database.EnsureCreatedAsync();

        // If there are already services in the database, skip seeding.
        // This prevents duplicate data if you restart the app.
        if (context.Services.Any())
        {
            return;
        }

        // Add sample services that represent a realistic microservice landscape.
        // These match the kind of services you'd see in a Kubernetes cluster.
        var services = new List<ServiceEntry>
        {
            new()
            {
                Name = "order-service",
                Namespace = "prod-east",
                Endpoint = "https://order-service.prod-east/health",
                Status = ServiceStatus.Healthy,
                LastChecked = DateTime.UtcNow.AddMinutes(-5)
            },
            new()
            {
                Name = "payment-gateway",
                Namespace = "prod-east",
                Endpoint = "https://payment-gateway.prod-east/health",
                Status = ServiceStatus.Healthy,
                LastChecked = DateTime.UtcNow.AddMinutes(-3)
            },
            new()
            {
                Name = "inventory-service",
                Namespace = "prod-west",
                Endpoint = "https://inventory-service.prod-west/health",
                Status = ServiceStatus.Degraded,
                LastChecked = DateTime.UtcNow.AddMinutes(-1),
                LastError = "High latency detected (>2s response time)"
            },
            new()
            {
                Name = "notification-service",
                Namespace = "staging",
                Endpoint = "https://notification-service.staging/health",
                Status = ServiceStatus.Unhealthy,
                LastChecked = DateTime.UtcNow.AddMinutes(-10),
                LastError = "Connection refused: service not responding on port 8080"
            },
            new()
            {
                Name = "auth-service",
                Namespace = "prod-east",
                Endpoint = "https://auth-service.prod-east/health",
                Status = ServiceStatus.Healthy,
                LastChecked = DateTime.UtcNow.AddMinutes(-2)
            },
            new()
            {
                Name = "analytics-pipeline",
                Namespace = "prod-west",
                Endpoint = "https://analytics-pipeline.prod-west/health",
                Status = ServiceStatus.Unknown,
                LastChecked = DateTime.MinValue // Never checked
            }
        };

        context.Services.AddRange(services);
        await context.SaveChangesAsync();
    }
}
