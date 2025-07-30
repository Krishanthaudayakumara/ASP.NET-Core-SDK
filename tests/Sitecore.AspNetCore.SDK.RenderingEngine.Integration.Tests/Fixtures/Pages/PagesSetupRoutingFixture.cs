using System.Net;
using Shouldly;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;

public class PagesSetupRoutingFixture(TestWebApplicationFactory<TestPagesProgram> factory) : IClassFixture<TestWebApplicationFactory<TestPagesProgram>>
{
    private readonly TestWebApplicationFactory<TestPagesProgram> _factory = factory;

    [Fact]
    public async Task ConfigRoute_MissingSecret_ReturnsBadRequest()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        string url = $"{TestConstants.ConfigRoute}?secret=";

        // Act
        HttpResponseMessage? response = await client.GetAsync(url);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfigRoute_InvalidSecret_ReturnsBadRequest()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        string url = $"{TestConstants.ConfigRoute}?secret=invalid_secret_value";

        // Act
        HttpResponseMessage? response = await client.GetAsync(url);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfigRoute_InvalidRequestOrigin_ReturnsBadRequest()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        string url = $"{TestConstants.ConfigRoute}?secret={TestConstants.JssEditingSecret}";
        client.DefaultRequestHeaders.Add("Origin", "http://invalid_origin_domain.com");

        // Act
        HttpResponseMessage? response = await client.GetAsync(url);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfigRoute_ValidCall_ReturnsCorrectObject()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        string url = $"{TestConstants.ConfigRoute}?secret={TestConstants.JssEditingSecret}";
        client.DefaultRequestHeaders.Add("Origin", "https://pages.sitecorecloud.io");

        // Act
        HttpResponseMessage? response = await client.GetAsync(url);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ShouldNotBeNull();
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        response.Headers.NonValidated["Content-Security-Policy"].ShouldBe("frame-ancestors 'self'  https://pages.sitecorecloud.io");
        response.Headers.NonValidated["Access-Control-Allow-Origin"].ShouldBe("https://pages.sitecorecloud.io");
        response.Headers.NonValidated["Access-Control-Allow-Methods"].ShouldBe("GET, POST, OPTIONS, PUT, PATCH, DELETE");
    }

    [Fact]
    public async Task RenderRoute_MissingSecret_ReturnsBadRequest()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        string url = $"{TestConstants.RenderRoute}?secret=";

        // Act
        HttpResponseMessage? response = await client.GetAsync(url);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RenderRoute_InvalidSecret_ReturnsBadRequest()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        string url = $"{TestConstants.RenderRoute}?secret=invalid_secret_value";

        // Act
        HttpResponseMessage? response = await client.GetAsync(url);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RenderRoute_ValidCall_ReturnsCorrectResponse()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        Guid itemId = Guid.NewGuid();
        string language = "en";
        string layoutKind = "final";
        string mode = "edit";
        string route = TestConstants.RenderRoute; // The controller needs to return a valid route in its RedirectResponse, so we're reusing the same route here instead of creating a fake one.
        string site = "siteA";
        string version = "1";
        string tenantId = "tenant1234";
        string url = $"{TestConstants.RenderRoute}?secret={TestConstants.JssEditingSecret}&sc_itemid={itemId}&sc_lang={language}&sc_layoutKind={layoutKind}&mode={mode}&sc_site={site}&sc_version={version}&tenant_id={tenantId}&route={route}";

        // Act
        HttpResponseMessage? response = await client.GetAsync(url);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }
}
