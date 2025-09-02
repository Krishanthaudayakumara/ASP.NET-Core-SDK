using System.Net;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using HtmlAgilityPack;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

// ReSharper disable StringLiteralTypo
namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

public class CustomModelContextBindingFixture : IClassFixture<TestWebApplicationFactory<TestCustomModelContextBindingProgram>>, IDisposable
{
    private readonly TestWebApplicationFactory<TestCustomModelContextBindingProgram> _factory;

    public CustomModelContextBindingFixture(TestWebApplicationFactory<TestCustomModelContextBindingProgram> factory)
    {
        _factory = factory;

        // Clear any previous state
        _factory.MockClientHandler.Requests.Clear();
        _factory.MockClientHandler.Responses.Clear();
    }

    [Fact]
    public async Task SitecoreLayoutModelBinders_BindDataCorrectly()
    {
        // Arrange
        string serObj = Serializer.Serialize(CannedResponses.WithNestedPlaceholder);
        JsonNode? jObject = JsonNode.Parse(serObj);
        JsonNode? contextJObject = jObject?["sitecore"]?["context"];
        contextJObject!["testClass1"] = new JsonObject([
            new KeyValuePair<string, JsonNode?>("testString", "stringExample"),
            new KeyValuePair<string, JsonNode?>("testInt", "123"),
            new KeyValuePair<string, JsonNode?>("testtime", "2020-12-08T13:09:44.1255842+02:00")
        ]);
        contextJObject["testClass2"] = new JsonObject([
            new KeyValuePair<string, JsonNode?>("testString", "stringExample2"),
            new KeyValuePair<string, JsonNode?>("testInt", "1234")
        ]);
        contextJObject["singleProperty"] = "SinglePropertyData";

        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jObject!.ToJsonString(Serializer.GetOptions()))
        });

        HttpClient client = _factory.CreateClient();

        // Act
        string response = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        HtmlDocument doc = new();
        doc.LoadHtml(response);
        HtmlNode? sectionNode = doc.DocumentNode.ChildNodes.First(n => n.HasClass("custom-model-context-component"));

        // Assert
        sectionNode.ChildNodes.First(n => n.Id.Equals("singleProp", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("SinglePropertyData");
        sectionNode.ChildNodes.First(n => n.Id.Equals("class1string", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("stringExample");
        sectionNode.ChildNodes.First(n => n.Id.Equals("class1date", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().NotBeNullOrWhiteSpace();
        sectionNode.ChildNodes.First(n => n.Id.Equals("class1int", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("123");
        sectionNode.ChildNodes.First(n => n.Id.Equals("class2string", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("stringExample2");
        sectionNode.ChildNodes.First(n => n.Id.Equals("class2int", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("1234");
        sectionNode.ChildNodes.First(n => n.Id.Equals("class1Indint", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("123");
        sectionNode.ChildNodes.First(n => n.Id.Equals("class1Indstr", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("stringExample");
        sectionNode.ChildNodes.First(n => n.Id.Equals("customCtxIndInt", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("123");
        sectionNode.ChildNodes.First(n => n.Id.Equals("customCtxIndStr", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("stringExample");
        sectionNode.ChildNodes.First(n => n.Id.Equals("individualProperty", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("SinglePropertyData");
    }

    public void Dispose()
    {
        // Don't dispose the factory - it's managed by the test framework when using IClassFixture
        GC.SuppressFinalize(this);
    }
}