namespace ServiceHealthApi.Models;

/// <summary>
/// Represents a microservice that we're tracking the health of.
/// This is our main entity — it gets stored in the database via Entity Framework.
/// </summary>
public class ServiceEntry
{
    public int Id { get; set; }

    /// <summary>The service name, e.g. "order-service"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The deployment namespace, e.g. "prod-east"</summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>The health check URL for this service</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Current known health status</summary>
    public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;

    /// <summary>When we last checked this service's health</summary>
    public DateTime LastChecked { get; set; }

    /// <summary>If the last check failed, this holds the error message</summary>
    public string? LastError { get; set; }
}

/// <summary>
/// Possible health states for a tracked service.
/// These map to typical infrastructure monitoring states.
/// </summary>
public enum ServiceStatus
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy
}
