using System.Globalization;
using System.Net;
using AwesomeAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Localization;

public class AdvanceLocalizationFixture : IClassFixture<TestWebApplicationFactory<TestPagesProgram>>
{
    private readonly WebApplicationFactory<TestPagesProgram> _factory;
    private readonly MockHttpMessageHandler _mockClientHandler;
    private readonly Uri _layoutServiceUri = new("http://layout.service");

    public AdvanceLocalizationFixture(TestWebApplicationFactory<TestPagesProgram> factory)
    {
        _factory = factory;
        _mockClientHandler = new MockHttpMessageHandler();
        _factory = factory
            .ConfigureServices(builder =>
            {
                builder.AddLocalization(options => options.ResourcesPath = "Resources");
                builder.AddSitecoreLayoutService()
                    .AddHttpHandler("mock", _ => new HttpClient(_mockClientHandler) { BaseAddress = _layoutServiceUri })
                    .AsDefaultHandler();

                builder.AddSitecoreRenderingEngine(options =>
                {
                    options.AddModelBoundView<ComponentModels.Component4>("Component-4", "Component4")
                        .AddDefaultComponentRenderer();
                });
                builder.AddMvc()
                    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseRequestLocalization(options =>
                {
                    List<CultureInfo> supportedCultures = [new("en"), new("da")];
                    options.DefaultRequestCulture = new RequestCulture(culture: "en", uiCulture: "en");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                    options.UseSitecoreRequestLocalization();
                });
                app.UseSitecoreRenderingEngine();

                app.UseEndpoints(endpoints =>
                {
                    // ReSharper disable once RouteTemplates.RouteParameterConstraintNotResolved - Custom constraint
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "content/{culture:culture}/{**sitecoreRoute}",
                        defaults: new { controller = "Home", action = "Index" });
                    endpoints.MapDefaultControllerRoute();
                });
            });}

    [Fact]
    public async Task LocalizationRouteProvider_SetsCorrectRequestsLanguage()
    {
        // Arrange
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithNestedPlaceholder))
        });

        HttpClient client = _factory.CreateClient();

        // Act
        await client.GetStringAsync(new Uri("content/da/UsingGlobalMiddleware", UriKind.Relative));

        _mockClientHandler.Requests.Single().RequestUri!.AbsoluteUri.Should().Contain("sc_lang=da");
    }

    [Fact]
    public async Task LocalizedRequest_PicksDefaultView()
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
        HtmlNode? sectionNode = doc.DocumentNode.ChildNodes.First(n => n.HasClass("component-4"));
        HtmlAttribute? attribute = sectionNode.Attributes.SingleOrDefault(a => a.Name == "data-language");

        attribute.Should().NotBeNull();
        attribute!.Value.Should().Be("en");
    }

    [Fact]
    public async Task LocalizedRequest_PicksLocalizedView()
    {
        // Arrange
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithNestedPlaceholder))
        });

        HttpClient client = _factory.CreateClient();

        // Act
        string response = await client.GetStringAsync(new Uri("content/da", UriKind.Relative));

        HtmlDocument doc = new();
        doc.LoadHtml(response);
        HtmlNode? sectionNode = doc.DocumentNode.ChildNodes.First(n => n.HasClass("component-4"));
        HtmlAttribute? attribute = sectionNode.Attributes.SingleOrDefault(a => a.Name == "data-language");

        attribute.Should().NotBeNull();
        attribute!.Value.Should().Be("da");
    }
}
