using System.Net;
using AwesomeAssertions;
using HtmlAgilityPack;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

public class ModelBindingErrorHandlingFixture : IClassFixture<TestWebApplicationFactory<TestModelBindingErrorHandlingProgram>>
{
    private readonly TestWebApplicationFactory<TestModelBindingErrorHandlingProgram> _factory;
    private readonly MockHttpMessageHandler _mockClientHandler;
    private readonly Uri _layoutServiceUri = new("http://layout.service");

    public ModelBindingErrorHandlingFixture(TestWebApplicationFactory<TestModelBindingErrorHandlingProgram> factory)
    {
        _factory = factory;
        _mockClientHandler = _factory.MockClientHandler;

        _factory.LayoutServiceUri = _layoutServiceUri;
    }

    [Fact]
    public async Task SitecoreLayoutModelBinders_HandleMissingDataCorrectly()
    {
        // Arrange
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithMissingData))
        });

        HttpClient client = _factory.CreateClient();

        // Act
        string response = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        HtmlDocument doc = new();
        doc.LoadHtml(response);
        HtmlNode? sectionNode = doc.DocumentNode.ChildNodes.Single(n => n.HasClass("component-with-missing-data"));
        HtmlNode? nestedSectionNode = sectionNode.ChildNodes.Single(n => n.HasClass("component-without-id"));

        // Assert
        sectionNode.ChildNodes.Single(n => n.Id.Equals("textField", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().BeEmpty();

        sectionNode.ChildNodes.Single(n => n.Id.Equals("routeProperty", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().BeEmpty();

        sectionNode.ChildNodes.Single(n => n.Id.Equals("routeField", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().BeEmpty();

        sectionNode.ChildNodes.Single(n => n.Id.Equals("contextProperty", StringComparison.OrdinalIgnoreCase)).InnerText.
            Should().BeEmpty();

        nestedSectionNode.ChildNodes.Single(n => n.Id.Equals("nestedRichTextField", StringComparison.OrdinalIgnoreCase)).InnerHtml.
            Should().BeEmpty();

        nestedSectionNode.ChildNodes.Single(n => n.Id.Equals("nestedTextField", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().BeEmpty();

        nestedSectionNode.ChildNodes.Single(n => n.Id.Equals("nestedComponentId", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().BeEmpty();
    }
}