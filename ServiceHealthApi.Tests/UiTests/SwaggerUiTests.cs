using Microsoft.Playwright;
using ServiceHealthApi.Tests.Fixtures;

namespace ServiceHealthApi.Tests.UiTests;

/// <summary>
/// Playwright browser tests against the Swagger UI.
///
/// These are the "real" Playwright learning tests — they open a browser,
/// navigate to pages, click buttons, fill forms, and assert on what's visible.
///
/// Key Playwright concepts demonstrated:
/// - Page navigation (GotoAsync)
/// - Locators (finding elements by CSS, text, role)
/// - Waiting (Playwright auto-waits for elements, but we can be explicit)
/// - Assertions (Expect().ToBeVisibleAsync, ToContainTextAsync)
/// - Interacting with forms (ClickAsync, FillAsync)
/// </summary>
[TestFixture]
public class SwaggerUiTests
{
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private string _baseUrl = null!;

    [SetUp]
    public async Task Setup()
    {
        _baseUrl = ApiFixture.BaseUrl;

        // Each test gets a fresh browser context (like an incognito window).
        // This isolates cookies, localStorage, etc. between tests.
        _context = await ApiFixture.Browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.CloseAsync();
    }

    [Test]
    public async Task SwaggerPage_Loads_ShowsApiTitle()
    {
        // Navigate to the Swagger UI (which is the root page in our config)
        await _page.GotoAsync(_baseUrl);

        // Wait for the Swagger UI to fully render by checking for the API title.
        // Playwright's Expect auto-waits up to the default timeout.
        var title = _page.Locator(".title");
        await Assertions.Expect(title).ToContainTextAsync("Service Health Tracker");
    }

    [Test]
    public async Task SwaggerPage_ListsAllEndpoints()
    {
        await _page.GotoAsync(_baseUrl);

        // Swagger groups endpoints by controller — check both groups are present.
        // The Locator finds elements matching the CSS selector.
        var content = _page.Locator("#swagger-ui");
        await Assertions.Expect(content).ToBeVisibleAsync();

        // Verify the endpoint sections exist (Swagger shows them as operation blocks)
        var operationBlocks = _page.Locator(".opblock");
        var count = await operationBlocks.CountAsync();

        // We have 6 Services endpoints + 3 HealthCheck endpoints = 9 total
        Assert.That(count, Is.GreaterThanOrEqualTo(9));
    }

    [Test]
    public async Task SwaggerPage_TryItOut_GetServices()
    {
        await _page.GotoAsync(_baseUrl);

        // Find the GET /api/services endpoint and expand it
        // Swagger renders each endpoint as an "opblock" with the HTTP method
        var getServicesBlock = _page.Locator("#operations-Services-get_api_Services");
        await getServicesBlock.ClickAsync();

        // Click the "Try it out" button
        var tryItOutButton = getServicesBlock.Locator("button.try-out__btn");
        await tryItOutButton.ClickAsync();

        // Click "Execute" to actually make the API call
        var executeButton = getServicesBlock.Locator("button.execute");
        await executeButton.ClickAsync();

        // Wait for and verify the response
        var responseBody = getServicesBlock.Locator(".responses-table .response-col_description pre");
        await Assertions.Expect(responseBody.First).ToBeVisibleAsync();

        // The response should contain our seeded service names
        var responseText = await responseBody.First.TextContentAsync();
        Assert.That(responseText, Does.Contain("order-service"));
    }

    [Test]
    public async Task SwaggerPage_TryItOut_CreateService()
    {
        await _page.GotoAsync(_baseUrl);

        // Find and expand the POST /api/services endpoint
        var postBlock = _page.Locator("#operations-Services-post_api_Services");
        await postBlock.ClickAsync();

        // Click "Try it out"
        var tryItOutButton = postBlock.Locator("button.try-out__btn");
        await tryItOutButton.ClickAsync();

        // Fill in the request body textarea with a new service
        var textarea = postBlock.Locator("textarea.body-param__text");
        await textarea.ClearAsync();
        await textarea.FillAsync("""
        {
            "name": "swagger-test-service",
            "namespace": "test",
            "endpoint": "https://swagger-test/health",
            "status": "Unknown"
        }
        """);

        // Execute the request
        var executeButton = postBlock.Locator("button.execute");
        await executeButton.ClickAsync();

        // Verify we got a 201 Created response
        var responseStatus = postBlock.Locator(".responses-table .response-col_status");
        await Assertions.Expect(responseStatus.First).ToBeVisibleAsync();

        var responseBody = postBlock.Locator(".responses-table .response-col_description pre");
        await Assertions.Expect(responseBody.First).ToBeVisibleAsync();
        var responseText = await responseBody.First.TextContentAsync();
        Assert.That(responseText, Does.Contain("swagger-test-service"));
    }

    [Test]
    public async Task SwaggerPage_TryItOut_DeleteService()
    {
        await _page.GotoAsync(_baseUrl);

        // Find and expand the DELETE endpoint
        var deleteBlock = _page.Locator("#operations-Services-delete_api_Services__id_");
        await deleteBlock.ClickAsync();

        // Click "Try it out"
        var tryItOutButton = deleteBlock.Locator("button.try-out__btn");
        await tryItOutButton.ClickAsync();

        // Fill in the ID parameter — use ID 1 (a seeded service)
        var idInput = deleteBlock.Locator("input[placeholder='id']");
        await idInput.ClearAsync();
        await idInput.FillAsync("1");

        // Execute the delete
        var executeButton = deleteBlock.Locator("button.execute");
        await executeButton.ClickAsync();

        // Verify we got a success response (204 No Content)
        var responseStatus = deleteBlock.Locator(".responses-table .response-col_status");
        await Assertions.Expect(responseStatus.First).ToBeVisibleAsync();
    }
}
