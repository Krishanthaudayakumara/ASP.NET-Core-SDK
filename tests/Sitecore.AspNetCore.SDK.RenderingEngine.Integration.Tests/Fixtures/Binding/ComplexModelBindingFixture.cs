using System.Net;
using AwesomeAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

/// <summary>
/// Fixture for testing complex model binding functionality.
/// </summary>
public class ComplexModelBindingFixture : IClassFixture<TestWebApplicationFactory<TestComplexModelBindingProgram>>, IDisposable
{
    private readonly TestWebApplicationFactory<TestComplexModelBindingProgram> _factory;
    private readonly MockHttpMessageHandler _mockClientHandler;
    private readonly Uri _layoutServiceUri = new("http://layout.service");

    /// <summary>
    /// Initializes a new instance of the <see cref="ComplexModelBindingFixture"/> class.
    /// </summary>
    /// <param name="factory">The test web application factory.</param>
    public ComplexModelBindingFixture(TestWebApplicationFactory<TestComplexModelBindingProgram> factory)
    {
        _factory = factory;
        _mockClientHandler = _factory.MockClientHandler;

        // Clear any previous state
        _factory.MockClientHandler.Requests.Clear();
        _factory.MockClientHandler.Responses.Clear();

        // Set the URIs for this test
        _factory.LayoutServiceUri = _layoutServiceUri;
    }

    /// <summary>
    /// Tests that Sitecore layout model binders bind data correctly for complex components.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SitecoreLayoutModelBinders_BindDataCorrectly()
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
        HtmlNode? sectionNode = doc.DocumentNode.ChildNodes.First(n => n.HasClass("complex-component"));

        // Assert
        sectionNode.ChildNodes.First(n => n.Id.Equals("fieldHeader", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be(TestConstants.PageTitle);

        sectionNode.ChildNodes.First(n => n.Id.Equals("routeField", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be(TestConstants.PageTitle);

        sectionNode.ChildNodes.First(n => n.Id.Equals("routeNestedField", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be(TestConstants.SearchKeywords);

        sectionNode.ChildNodes.First(n => n.Id.Equals("componentProperty", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be("Complex-Component");

        sectionNode.ChildNodes.First(n => n.Id.Equals("routeProperty", StringComparison.OrdinalIgnoreCase)).InnerText.
            Should().Be("styleguide");

        sectionNode.ChildNodes.First(n => n.Id.Equals("fieldContent", StringComparison.OrdinalIgnoreCase)).InnerHtml.
            Should().Be(TestConstants.RichTextFieldValue1);

        sectionNode.ChildNodes.First(n => n.Id.Equals("contextProperty", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Contain("False");

        sectionNode.ChildNodes.First(n => n.Id.Equals("nestedComponentHeaderField", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be(TestConstants.PageTitle);

        sectionNode.ChildNodes.First(n => n.Id.Equals("nestedComponentHeader2Field", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Contain(TestConstants.TestFieldValue);

        sectionNode.ChildNodes.First(n => n.Id.Contains("customField", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Contain(TestConstants.TestFieldValue + "custom");

        sectionNode.ChildNodes.First(n => n.Id.Equals("paramContent", StringComparison.OrdinalIgnoreCase)).InnerText
            .Should().Be(TestConstants.TestParamNameValue);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Don't dispose the factory - it's managed by the test framework when using IClassFixture
        GC.SuppressFinalize(this);
    }
}