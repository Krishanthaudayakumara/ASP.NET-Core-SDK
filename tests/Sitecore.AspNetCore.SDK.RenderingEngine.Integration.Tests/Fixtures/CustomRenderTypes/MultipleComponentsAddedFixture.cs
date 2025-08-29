using System.Net;
using System.Text.Encodings.Web;
using AwesomeAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.CustomRenderTypes;

public class MultipleComponentsAddedFixture : IClassFixture<TestWebApplicationFactory<TestPagesProgram>>
{
    private readonly WebApplicationFactory<TestPagesProgram> _factory;
    private readonly MockHttpMessageHandler _mockClientHandler;
    private readonly Uri _layoutServiceUri = new("http://layout.service");

    public MultipleComponentsAddedFixture(TestWebApplicationFactory<TestPagesProgram> factory)
    {
        _factory = factory;
        _mockClientHandler = new MockHttpMessageHandler();
        _factory = factory
            .ConfigureServices(builder =>
            {
                builder
                    .AddSitecoreLayoutService()
                    .AddHttpHandler("mock", _ => new HttpClient(_mockClientHandler) { BaseAddress = _layoutServiceUri })
                    .AsDefaultHandler();
                builder.AddSitecoreRenderingEngine(options =>
                {
                    options
                        .AddModelBoundView<ComponentModels.Component3>(name => name.Equals("Component-3", StringComparison.OrdinalIgnoreCase), "Component3")
                        .AddModelBoundView<ComponentModels.Component3>(name => name.Equals("Component-6", StringComparison.OrdinalIgnoreCase), "Component6")
                        .AddDefaultComponentRenderer();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseSitecoreRenderingEngine();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapDefaultControllerRoute();
                });
            });}

    [Fact]
    public async Task CustomRenderTypes_MultipleComponentsBoundsInCorrectOrder()
    {
        // Arrange
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithNestedPlaceholder))
        });

        HttpClient client = _factory.CreateClient();

        // Act
        string response = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        HtmlDocument doc = new();
        doc.LoadHtml(response);
        HtmlNodeCollection? docNodes = doc.DocumentNode.ChildNodes;

        // Assert
        docNodes.GetNodeIndex(docNodes.First(n => n.HasClass("component-3")))
            .Should()
            .BeLessThan(docNodes.GetNodeIndex(docNodes.First(n => n.HasClass("component-6"))));

        HtmlNode? sectionNode = docNodes.First(n => n.HasClass("component-3"));
        sectionNode.ChildNodes.First(n => n.Name.Equals("h1", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be(HtmlEncoder.Default.Encode(TestConstants.TestFieldValue));

        sectionNode = doc.DocumentNode.ChildNodes.First(n => n.HasClass("component-6"));
        sectionNode.ChildNodes.First(n => n.Name.Equals("textarea", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be(HtmlEncoder.Default.Encode(TestConstants.TestFieldValue + " from Component-6"));
    }
}
