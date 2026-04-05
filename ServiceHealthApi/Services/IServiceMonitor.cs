using ServiceHealthApi.Models;

namespace ServiceHealthApi.Services;

/// <summary>
/// Interface for the service monitoring component.
///
/// WHY AN INTERFACE? This is a core Dependency Injection (DI) pattern:
/// - Controllers depend on IServiceMonitor, not the concrete ServiceMonitor class.
/// - This means you can swap in a different implementation without changing any controller code.
/// - In tests, you could inject a mock that returns predictable results.
/// - In production, you might swap in a version that makes real HTTP calls.
///
/// The "I" prefix is a C# convention for interfaces.
/// </summary>
public interface IServiceMonitor
{
    /// <summary>
    /// Check the health of a single service.
    /// Returns a HealthStatus with the check result.
    /// </summary>
    Task<HealthStatus> CheckServiceAsync(ServiceEntry service);

    /// <summary>
    /// Check the health of all tracked services.
    /// Returns a collection of HealthStatus results.
    /// </summary>
    Task<IEnumerable<HealthStatus>> CheckAllServicesAsync();
}
