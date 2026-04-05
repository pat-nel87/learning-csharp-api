using System.Net;
using System.Net.Http.Json;
using ServiceHealthApi.Models;
using ServiceHealthApi.Tests.Fixtures;

namespace ServiceHealthApi.Tests.ApiTests;

/// <summary>
/// Tests for the /api/services CRUD endpoints.
///
/// These tests use HttpClient directly (no browser needed).
/// They demonstrate:
/// - Sending different HTTP methods (GET, POST, PUT, DELETE)
/// - Deserializing JSON responses into C# objects
/// - Asserting on HTTP status codes and response bodies
/// - Test isolation with a fresh database per test class
/// </summary>
[TestFixture]
public class ServicesApiTests
{
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _client = ApiFixture.HttpClient;
    }

    [Test]
    public async Task GetAllServices_ReturnsSeededData()
    {
        // Arrange & Act — GET all services
        var response = await _client.GetAsync("/api/services");

        // Assert — should return 200 OK with the seeded services
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var services = await response.Content.ReadFromJsonAsync<List<ServiceEntry>>();
        Assert.That(services, Is.Not.Null);
        Assert.That(services!.Count, Is.GreaterThanOrEqualTo(6));

        // Verify some of the seeded service names exist
        var names = services.Select(s => s.Name).ToList();
        Assert.That(names, Does.Contain("order-service"));
        Assert.That(names, Does.Contain("payment-gateway"));
        Assert.That(names, Does.Contain("auth-service"));
    }

    [Test]
    public async Task GetServiceById_ReturnsCorrectService()
    {
        // Act — GET the first service
        var response = await _client.GetAsync("/api/services/1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var service = await response.Content.ReadFromJsonAsync<ServiceEntry>();
        Assert.That(service, Is.Not.Null);
        Assert.That(service!.Id, Is.EqualTo(1));
        Assert.That(service.Name, Is.Not.Empty);
        Assert.That(service.Endpoint, Is.Not.Empty);
    }

    [Test]
    public async Task GetServiceById_InvalidId_Returns404()
    {
        // Act — request a service that doesn't exist
        var response = await _client.GetAsync("/api/services/999");

        // Assert — should return 404 Not Found
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateService_ReturnsCreatedService()
    {
        // Arrange — create a new service to add
        var newService = new ServiceEntry
        {
            Name = "test-service-create",
            Namespace = "test-ns",
            Endpoint = "https://test-service/health",
            Status = ServiceStatus.Unknown
        };

        // Act — POST to create
        var response = await _client.PostAsJsonAsync("/api/services", newService);

        // Assert — should return 201 Created
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var created = await response.Content.ReadFromJsonAsync<ServiceEntry>();
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Name, Is.EqualTo("test-service-create"));
        Assert.That(created.Id, Is.GreaterThan(0));
    }

    [Test]
    public async Task UpdateService_ModifiesExistingService()
    {
        // Arrange — first create a service to update
        var newService = new ServiceEntry
        {
            Name = "test-service-update",
            Namespace = "test-ns",
            Endpoint = "https://test-service-update/health",
            Status = ServiceStatus.Unknown
        };
        var createResponse = await _client.PostAsJsonAsync("/api/services", newService);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceEntry>();

        // Act — PUT to update the name
        created!.Name = "updated-service-name";
        var updateResponse = await _client.PutAsJsonAsync($"/api/services/{created.Id}", created);

        // Assert — should return 204 No Content
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verify the update persisted
        var getResponse = await _client.GetAsync($"/api/services/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ServiceEntry>();
        Assert.That(updated!.Name, Is.EqualTo("updated-service-name"));
    }

    [Test]
    public async Task DeleteService_RemovesService()
    {
        // Arrange — create a service to delete
        var newService = new ServiceEntry
        {
            Name = "test-service-delete",
            Namespace = "test-ns",
            Endpoint = "https://test-service-delete/health",
            Status = ServiceStatus.Unknown
        };
        var createResponse = await _client.PostAsJsonAsync("/api/services", newService);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceEntry>();

        // Act — DELETE the service
        var deleteResponse = await _client.DeleteAsync($"/api/services/{created!.Id}");

        // Assert — should return 204
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verify it's gone — should return 404
        var getResponse = await _client.GetAsync($"/api/services/{created.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task FilterByStatus_ReturnsMatchingServices()
    {
        // Act — filter for Healthy services
        var response = await _client.GetAsync("/api/services/status/Healthy");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var services = await response.Content.ReadFromJsonAsync<List<ServiceEntry>>();
        Assert.That(services, Is.Not.Null);
        Assert.That(services!.Count, Is.GreaterThan(0));

        // Every returned service should have Healthy status
        Assert.That(services.All(s => s.Status == ServiceStatus.Healthy), Is.True);
    }
}
