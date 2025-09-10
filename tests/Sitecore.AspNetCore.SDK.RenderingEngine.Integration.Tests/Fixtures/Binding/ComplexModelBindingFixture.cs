using System.Net;
using AwesomeAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.AspNetCore.SDK.RenderingEngine.Configuration;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.TestBootstrapping;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

public class ComplexModelBindingFixture : IDisposable
{
    private readonly Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<TestPagesProgram> _factory;
    private readonly MockHttpMessageHandler _mockClientHandler;
    private readonly Uri _layoutServiceUri = new("http://layout.service");

    public ComplexModelBindingFixture()
    {
        _mockClientHandler = new MockHttpMessageHandler();

        // Use the existing TestWebApplicationFactory and register test-specific services via WithWebHostBuilder so we don't change the global factory behavior.
        _factory = new TestWebApplicationFactory<TestPagesProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services
                        .AddSitecoreLayoutService()
                        .AddHttpHandler("mock", _ => new HttpClient(_mockClientHandler) { BaseAddress = _layoutServiceUri })
                        .AsDefaultHandler();

                    // Register a bootstrapper instance which TestPagesProgram will discover and execute at startup. The bootstrapper will configure rendering engine options for this fixture.
                    var bootstrapper = new BindingTestBootstrapper(
                        configureOptions: options =>
                        {
                            options.RendererRegistry.Clear();
                            options
                                .AddModelBoundView<ComponentModels.ComplexComponent>(name => name.Equals("Complex-Component", StringComparison.OrdinalIgnoreCase), "ComplexComponent")
                                .AddDefaultComponentRenderer()
                                .AddDefaultPartialView("_ComponentNotFound");
                        });

                    services.AddSingleton<ITestBootstrapper>(bootstrapper);
                });
            });
    }

    [Fact]
    public async Task SitecoreLayoutModelBinders_BindDataCorrectly()
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithNestedPlaceholder))
        });

        HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost") });

        // Request the Home controller so the NestedPlaceholderPageLayout is returned.
        string response = await client.GetStringAsync(new Uri("/Home", UriKind.Relative));

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

    public void Dispose()
    {
    _factory.Dispose();
    _mockClientHandler.Dispose();
    GC.SuppressFinalize(this);
    }
}