using System.Net;
using System.Text.Encodings.Web;
using AwesomeAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.CustomRenderTypes;

public class MultipleComponentsAddedFixture : IClassFixture<TestWebApplicationFactory<TestMultipleComponentsAddedProgram>>
{
    private readonly TestWebApplicationFactory<TestMultipleComponentsAddedProgram> _factory;
    private readonly MockHttpMessageHandler _mockClientHandler;
    private readonly Uri _layoutServiceUri = new("http://layout.service");

    public MultipleComponentsAddedFixture(TestWebApplicationFactory<TestMultipleComponentsAddedProgram> factory)
    {
        _factory = factory;
        _mockClientHandler = _factory.MockClientHandler;

        _factory.LayoutServiceUri = _layoutServiceUri;
    }

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