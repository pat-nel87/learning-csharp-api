using System.Net;
using System.Net.Http.Json;
using ServiceHealthApi.Models;
using ServiceHealthApi.Tests.Fixtures;

namespace ServiceHealthApi.Tests.ApiTests;

/// <summary>
/// Tests for the /api/healthcheck endpoints.
///
/// These tests exercise the health check simulation, which includes
/// random latency and occasional failures — so some assertions are
/// intentionally flexible (checking structure, not exact values).
/// </summary>
[TestFixture]
public class HealthCheckApiTests
{
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _client = ApiFixture.HttpClient;
    }

    [Test]
    public async Task HealthEndpoint_ReturnsOk()
    {
        // Act — hit the API's own health endpoint
        var response = await _client.GetAsync("/api/healthcheck");

        // Assert — should always return 200
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify the response has the expected structure
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Healthy"));
        Assert.That(content, Does.Contain("timestamp"));
    }

    [Test]
    public async Task CheckService_ReturnsHealthStatus()
    {
        // Act — check the health of service ID 1
        var response = await _client.GetAsync("/api/healthcheck/check/1");

        // Assert — should return a HealthStatus object
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var status = await response.Content.ReadFromJsonAsync<HealthStatus>();
        Assert.That(status, Is.Not.Null);
        Assert.That(status!.ServiceName, Is.Not.Empty);
        Assert.That(status.ResponseTimeMs, Is.GreaterThan(0));
        Assert.That(status.CheckedAt, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public async Task CheckAllServices_UpdatesStatuses()
    {
        // Act — check all services (POST because it has side effects)
        var response = await _client.PostAsync("/api/healthcheck/check-all", null);

        // Assert — should return a list of health statuses
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var statuses = await response.Content.ReadFromJsonAsync<List<HealthStatus>>();
        Assert.That(statuses, Is.Not.Null);
        Assert.That(statuses!.Count, Is.GreaterThan(0));

        // Each result should have a service name and a valid check time
        foreach (var status in statuses)
        {
            Assert.That(status.ServiceName, Is.Not.Empty);
            Assert.That(status.CheckedAt, Is.Not.EqualTo(default(DateTime)));
        }
    }
}
