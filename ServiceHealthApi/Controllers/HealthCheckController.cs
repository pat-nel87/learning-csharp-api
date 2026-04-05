using Microsoft.AspNetCore.Mvc;
using ServiceHealthApi.Data;
using ServiceHealthApi.Models;
using ServiceHealthApi.Services;

namespace ServiceHealthApi.Controllers;

/// <summary>
/// Health check endpoints that simulate checking service health.
///
/// This controller demonstrates Dependency Injection with an interface:
/// it receives IServiceMonitor (not ServiceMonitor directly), so the concrete
/// implementation can be swapped without changing this code.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IServiceMonitor _monitor;

    // Two dependencies injected: the database context AND the service monitor.
    // ASP.NET's DI container resolves both automatically.
    public HealthCheckController(AppDbContext context, IServiceMonitor monitor)
    {
        _context = context;
        _monitor = monitor;
    }

    /// <summary>
    /// GET /api/healthcheck — Simple liveness check for the API itself.
    /// Always returns 200 OK. This is the kind of endpoint a load balancer
    /// would hit to know if your API is running.
    /// </summary>
    [HttpGet]
    public ActionResult GetHealthAsync()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// GET /api/healthcheck/check/{id} — Simulate checking a specific service's health.
    /// Looks up the service by ID, then runs a simulated health check.
    /// </summary>
    [HttpGet("check/{id}")]
    public async Task<ActionResult<HealthStatus>> CheckServiceAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service is null)
        {
            return NotFound();
        }

        var result = await _monitor.CheckServiceAsync(service);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/healthcheck/check-all — Simulate checking ALL tracked services.
    /// This updates each service's status in the database and returns all results.
    /// POST because it has side effects (updates database state).
    /// </summary>
    [HttpPost("check-all")]
    public async Task<ActionResult<IEnumerable<HealthStatus>>> CheckAllServicesAsync()
    {
        var results = await _monitor.CheckAllServicesAsync();
        return Ok(results);
    }
}
