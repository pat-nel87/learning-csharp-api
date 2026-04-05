namespace ServiceHealthApi.Models;

/// <summary>
/// A DTO (Data Transfer Object) returned when you check a service's health.
/// This is separate from ServiceEntry because it represents a point-in-time check result,
/// not the persisted entity. Keeping these separate is a common API pattern.
/// </summary>
public class HealthStatus
{
    /// <summary>Name of the service that was checked</summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>The result of the health check</summary>
    public ServiceStatus Status { get; set; }

    /// <summary>When this check was performed</summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>How long the check took in milliseconds</summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>Error details if the check failed</summary>
    public string? ErrorMessage { get; set; }
}
