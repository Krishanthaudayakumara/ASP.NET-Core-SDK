using System.Net;
using AwesomeAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.CustomRenderTypes;

public class PartialViewRendererFixture(TestWebApplicationFactory<TestWebApplicationProgram> factory) : IClassFixture<TestWebApplicationFactory<TestWebApplicationProgram>>, IDisposable
{
    private readonly MockHttpMessageHandler _mockClientHandler = new();
    private readonly Uri _layoutServiceUri = new("http://layout.service");

    [Fact]
    public async Task CustomRenderTypes_PartialViewRendersCorrectly()
    {
        // Arrange
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithNestedPlaceholder))
        });

        HttpClient client = BuildPartialViewWebApplicationFactory().CreateClient();

        // Act
        string response = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        HtmlDocument doc = new();
        doc.LoadHtml(response);
        HtmlNode? sectionNode = doc.DocumentNode.ChildNodes.First(n => n.HasClass("component-6"));

        // Assert
        sectionNode.ChildNodes.First(n => n.Name.Equals("textarea", StringComparison.OrdinalIgnoreCase)).InnerHtml
            .Should().Be(TestConstants.TestFieldValue + " from Component-6");
    }

    public void Dispose()
    {
        _mockClientHandler.Dispose();
        GC.SuppressFinalize(this);
    }

    private WebApplicationFactory<TestWebApplicationProgram> BuildPartialViewWebApplicationFactory()
    {
        return factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services
                        .AddSitecoreLayoutService()
                        .AddHttpHandler("mock", _ => new HttpClient(_mockClientHandler) { BaseAddress = _layoutServiceUri })
                        .AsDefaultHandler();

                    services.AddSitecoreRenderingEngine(options =>
                    {
                        options
                            .AddPartialView(name => name.Equals("Component-6", StringComparison.OrdinalIgnoreCase), "_PartialView")
                            .AddDefaultComponentRenderer();
                    });
                });

                builder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseSitecoreRenderingEngine();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapDefaultControllerRoute();
                    });
                });
            });
    }
}