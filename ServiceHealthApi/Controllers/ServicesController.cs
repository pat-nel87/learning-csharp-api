using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHealthApi.Data;
using ServiceHealthApi.Models;

namespace ServiceHealthApi.Controllers;

/// <summary>
/// CRUD controller for managing tracked services.
///
/// This demonstrates the standard ASP.NET Core controller pattern:
/// - [ApiController] enables automatic model validation and binding
/// - [Route] sets the base URL path for all actions in this controller
/// - Each method maps to an HTTP verb (GET, POST, PUT, DELETE)
/// - Return types use ActionResult for proper HTTP status codes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _context;

    // Constructor injection — ASP.NET's DI provides the database context
    public ServicesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET /api/services — Returns all tracked services.
    /// ToListAsync() executes the query and returns results asynchronously.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceEntry>>> GetAllServicesAsync()
    {
        var services = await _context.Services.ToListAsync();
        return Ok(services);
    }

    /// <summary>
    /// GET /api/services/{id} — Returns a single service by its ID.
    /// FindAsync is an EF Core method that looks up by primary key.
    /// Returns 404 if the service doesn't exist.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceEntry>> GetServiceByIdAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service is null)
        {
            return NotFound();
        }

        return Ok(service);
    }

    /// <summary>
    /// POST /api/services — Creates a new service entry.
    /// [FromBody] tells ASP.NET to deserialize the request body JSON into a ServiceEntry.
    /// Returns 201 Created with the location of the new resource.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ServiceEntry>> CreateServiceAsync([FromBody] ServiceEntry service)
    {
        // Reset Id to 0 so EF Core auto-generates it
        service.Id = 0;
        service.LastChecked = DateTime.UtcNow;

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        // CreatedAtAction returns 201 with a Location header pointing to the new resource.
        // The first argument names the GET action, the second provides route values,
        // and the third is the response body.
        return CreatedAtAction(
            nameof(GetServiceByIdAsync),
            new { id = service.Id },
            service);
    }

    /// <summary>
    /// PUT /api/services/{id} — Updates an existing service.
    /// PUT replaces the entire resource (as opposed to PATCH which does partial updates).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateServiceAsync(int id, [FromBody] ServiceEntry updatedService)
    {
        var existing = await _context.Services.FindAsync(id);

        if (existing is null)
        {
            return NotFound();
        }

        // Update the fields on the tracked entity.
        // EF Core detects these changes and generates an UPDATE statement on SaveChanges.
        existing.Name = updatedService.Name;
        existing.Namespace = updatedService.Namespace;
        existing.Endpoint = updatedService.Endpoint;
        existing.Status = updatedService.Status;
        existing.LastError = updatedService.LastError;

        await _context.SaveChangesAsync();

        return NoContent(); // 204 — success, no response body needed
    }

    /// <summary>
    /// DELETE /api/services/{id} — Removes a service.
    /// Returns 404 if it doesn't exist, 204 if successfully deleted.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteServiceAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service is null)
        {
            return NotFound();
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// GET /api/services/status/{status} — Filter services by health status.
    /// Demonstrates enum parsing from route parameters and LINQ Where clause.
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<ServiceEntry>>> GetServicesByStatusAsync(ServiceStatus status)
    {
        var services = await _context.Services
            .Where(s => s.Status == status)
            .ToListAsync();

        return Ok(services);
    }
}
