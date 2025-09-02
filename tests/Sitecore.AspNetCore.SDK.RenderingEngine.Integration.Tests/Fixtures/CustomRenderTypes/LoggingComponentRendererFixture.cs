using System.Net;
using AwesomeAssertions;
using HtmlAgilityPack;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Logging;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.CustomRenderTypes;

public class LoggingComponentRendererFixture : IClassFixture<TestWebApplicationFactory<TestLoggingComponentRendererProgram>>, IDisposable
{
    private readonly TestWebApplicationFactory<TestLoggingComponentRendererProgram> _factory;

    public LoggingComponentRendererFixture(TestWebApplicationFactory<TestLoggingComponentRendererProgram> factory)
    {
        _factory = factory;

        // Clear any previous state
        _factory.MockClientHandler.Requests.Clear();
        _factory.MockClientHandler.Responses.Clear();

        // Clear previous log entries
        InMemoryLog.Log.Clear();
    }

    [Fact]
    public async Task CustomRenderTypes_DefaultRendererWritesToLogWithoutRenderingHtml()
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
        HtmlNode? sectionNode = doc.DocumentNode.ChildNodes.FirstOrDefault(n => n.HasClass("component-6"));

        // Assert
        sectionNode.Should().BeNull();

        InMemoryLog.Log.Should().Contain("LoggingComponentRenderer: Render method called. Component name: Component-6");
    }

    public void Dispose()
    {
        // Don't dispose the factory - it's managed by the test framework when using IClassFixture
        GC.SuppressFinalize(this);
    }
}