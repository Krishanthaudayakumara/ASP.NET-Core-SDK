using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Interfaces;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Request;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures;

public class RequestHeadersValidationFixture
{
    [Fact]
    public async Task Request_WithNonValidatedHeaders_HeadersAreProperlyValidated()
    {
        // Arrange
        using TestWebApplicationFactory<TestPagesProgram> factory = CreateFactory(["User-Agent"]);
        ISitecoreLayoutClient layoutClient = factory.Services.GetRequiredService<ISitecoreLayoutClient>();

        SitecoreLayoutRequest request = new SitecoreLayoutRequest()
            .Path("test");

        // Act
        SitecoreLayoutResponse response = await layoutClient.Request(request);

        // Assert
        response.Should().NotBeNull();
        response.Errors.FirstOrDefault(error => error.InnerException!.Message == "The format of value 'site;core' is invalid.").Should().Be(null);
        object? headerKeys = response.Request["sc_request_headers_key"];

        Dictionary<string, string[]>? userAgentHeader = headerKeys as Dictionary<string, string[]>;
        userAgentHeader!["User-Agent"][0].Should().Be("site;core");
    }

    [Fact]
    public async Task Request_WithoutNonValidatedHeaders_ErrorThrown()
    {
        // Arrange
        using TestWebApplicationFactory<TestPagesProgram> factory = CreateFactory([]);
        ISitecoreLayoutClient layoutClient = factory.Services.GetRequiredService<ISitecoreLayoutClient>();

        SitecoreLayoutRequest request = new SitecoreLayoutRequest()
            .Path("test");

        // Act
        SitecoreLayoutResponse response = await layoutClient.Request(request);

        // Assert
        response.Should().NotBeNull();
        response.Errors.FirstOrDefault(error => error.InnerException!.Message == "The format of value 'site;core' is invalid.").Should().NotBe(null);
    }

    private static TestWebApplicationFactory<TestPagesProgram> CreateFactory(string[] nonValidatedHeaders)
    {
        MockHttpMessageHandler clientHandler = new();
        Dictionary<string, string[]> headers = new()
        {
            { "User-Agent", ["site;core"] }
        };

        return new TestWebApplicationFactory<TestPagesProgram>()
            .ConfigureServices(builder =>
            {
                ISitecoreLayoutClientBuilder lsc = builder
                    .AddSitecoreLayoutService();

                lsc.AddHttpHandler("mock", _ => new HttpClient(clientHandler) { BaseAddress = new Uri("http://layout.service") }, nonValidatedHeaders)
                    .WithRequestOptions(request =>
                    {
                        request["sc_request_headers_key"] = headers;
                        request["key3"] = "value4";
                    })
                    .AsDefaultHandler();

                builder
                    .AddSitecoreRenderingEngine();
            })
            .Configure(app =>
            {
                app.UseSitecoreRenderingEngine();
            });
    }
}