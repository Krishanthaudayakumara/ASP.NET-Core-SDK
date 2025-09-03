using System.Net;
using AwesomeAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Xunit;

// ReSharper disable StringLiteralTypo
namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

public class CustomResolverBindingFixture : IClassFixture<TestWebApplicationFactory<TestCustomResolverBindingProgram>>
{
    private readonly TestWebApplicationFactory<TestCustomResolverBindingProgram> _factory;

    public CustomResolverBindingFixture(TestWebApplicationFactory<TestCustomResolverBindingProgram> factory)
    {
        _factory = factory;
        _factory.MockClientHandler.Requests.Clear();
        _factory.MockClientHandler.Responses.Clear();
    }

    [Fact]
    public async Task SitecoreLayoutModelBinders_BindDataCorrectly()
    {
        // Arrange
        string json = await File.ReadAllTextAsync("./Json/headlessSxa.json");
        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        HttpClient client = _factory.CreateClient();

        // Act
        string response = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        HtmlDocument doc = new();
        doc.LoadHtml(response);
        HtmlNode? sectionNode = doc.DocumentNode.ChildNodes["head"].ChildNodes.First(n => n.HasClass("custom-resolver"));

        // Assert
        sectionNode.ChildNodes["ul"].ChildNodes.Count(c => c.Name.Equals("li")).Should().Be(1);
        sectionNode.ChildNodes["ul"].ChildNodes["li"].ChildNodes.Count(c => c.Name.Equals("span")).Should().Be(1);
        sectionNode.ChildNodes["ul"].ChildNodes["li"].ChildNodes["span"].InnerText.Should().Be("Home");

        sectionNode.ChildNodes["ul"].ChildNodes["li"].ChildNodes["ul"].ChildNodes.Count(c => c.Name.Equals("li")).Should().Be(1);
        sectionNode.ChildNodes["ul"].ChildNodes["li"].ChildNodes["ul"].ChildNodes["li"].ChildNodes["span"].InnerText.Should().Be("About");
    }
}