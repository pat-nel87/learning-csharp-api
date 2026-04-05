using Microsoft.EntityFrameworkCore;
using ServiceHealthApi.Data;
using ServiceHealthApi.Models;

namespace ServiceHealthApi.Services;

/// <summary>
/// Simulated service health checker.
///
/// In a real system, this would make HTTP calls to each service's health endpoint.
/// Here we simulate it with random delays and occasional failures, which makes
/// the tests more interesting and realistic without requiring actual network calls.
///
/// Notice how this class receives AppDbContext through its constructor — that's
/// Dependency Injection at work. The DI container creates and passes the context
/// automatically when a controller needs an IServiceMonitor.
/// </summary>
public class ServiceMonitor : IServiceMonitor
{
    private readonly AppDbContext _context;
    private readonly Random _random = new();

    // Constructor injection: ASP.NET's DI container will provide the AppDbContext
    public ServiceMonitor(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Simulates checking a single service's health.
    /// - Adds random latency (50-500ms) to simulate network calls
    /// - ~10% chance of returning Degraded or Unhealthy for realism
    /// - Updates the entity in the database with the new status
    /// </summary>
    public async Task<HealthStatus> CheckServiceAsync(ServiceEntry service)
    {
        // Simulate network latency — in a real system this would be an HTTP call
        var latencyMs = _random.Next(50, 500);
        await Task.Delay(latencyMs);

        // Determine the health status with ~10% chance of degraded/unhealthy
        var roll = _random.Next(100);
        var status = roll switch
        {
            < 5 => ServiceStatus.Unhealthy,    // 5% chance
            < 10 => ServiceStatus.Degraded,     // 5% chance
            _ => ServiceStatus.Healthy           // 90% chance
        };

        string? errorMessage = status switch
        {
            ServiceStatus.Unhealthy => "Connection timed out after 30s",
            ServiceStatus.Degraded => $"Slow response: {latencyMs * 3}ms",
            _ => null
        };

        // Update the entity in the database — EF Core tracks changes automatically.
        // When we call SaveChangesAsync later, it generates the SQL UPDATE statement.
        service.Status = status;
        service.LastChecked = DateTime.UtcNow;
        service.LastError = errorMessage;

        // Only save if the entity is being tracked by EF Core
        if (_context.Entry(service).State != EntityState.Detached)
        {
            await _context.SaveChangesAsync();
        }

        return new HealthStatus
        {
            ServiceName = service.Name,
            Status = status,
            CheckedAt = DateTime.UtcNow,
            ResponseTimeMs = latencyMs,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Checks all tracked services and returns their health statuses.
    /// Uses async/await with a foreach loop — each check runs sequentially.
    /// (In production you might use Task.WhenAll for parallelism, but sequential
    /// is clearer for learning.)
    /// </summary>
    public async Task<IEnumerable<HealthStatus>> CheckAllServicesAsync()
    {
        var services = await _context.Services.ToListAsync();
        var results = new List<HealthStatus>();

        foreach (var service in services)
        {
            var result = await CheckServiceAsync(service);
            results.Add(result);
        }

        return results;
    }
}
