using System.Net;
using AwesomeAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.CustomRenderTypes;

public class PartialViewRendererFixture : IClassFixture<TestWebApplicationFactory<TestPartialViewRendererProgram>>
{
    private readonly TestWebApplicationFactory<TestPartialViewRendererProgram> _factory;

    public PartialViewRendererFixture(TestWebApplicationFactory<TestPartialViewRendererProgram> factory)
    {
        _factory = factory;

        // Clear any previous state
        _factory.MockClientHandler.Requests.Clear();
        _factory.MockClientHandler.Responses.Clear();
    }

    [Fact]
    public async Task CustomRenderTypes_PartialViewRendersCorrectly()
    {
        // Arrange
        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithNestedPlaceholder))
        });

        HttpClient client = _factory.CreateClient();

        // Act
        string response = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        HtmlDocument doc = new();
        doc.LoadHtml(response);
        HtmlNode? sectionNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.HasClass("component-6"));

        // Assert
        sectionNode.ChildNodes.First(n => n.Name.Equals("textarea", StringComparison.OrdinalIgnoreCase)).InnerHtml
            .Should().Be(TestConstants.TestFieldValue + " from Component-6");
    }
}