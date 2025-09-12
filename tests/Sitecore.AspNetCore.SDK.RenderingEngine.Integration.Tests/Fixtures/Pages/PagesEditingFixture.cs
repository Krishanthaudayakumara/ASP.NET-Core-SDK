using System.Net;
using AwesomeAssertions;
using GraphQL;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using NSubstitute;
using Sitecore.AspNetCore.SDK.GraphQL.Request;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.Pages.Configuration;
using Sitecore.AspNetCore.SDK.Pages.Extensions;
using Sitecore.AspNetCore.SDK.Pages.Middleware;
using Sitecore.AspNetCore.SDK.Pages.Request.Handlers.GraphQL;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;

public class PagesEditingFixture(TestWebApplicationFactory<TestWebApplicationProgram> factory) : IClassFixture<TestWebApplicationFactory<TestWebApplicationProgram>>
{
    [Fact]
    public async Task EditingRequest_ValidRequest_ReturnsChromeDecoratedResponse()
    {
        // Arrange
        factory.MockGraphQLClient.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLHttpRequestWithHeaders>()).Returns(TestConstants.SimpleEditingLayoutQueryResponse);
        factory.MockGraphQLClient.SendQueryAsync<EditingDictionaryResponse>(Arg.Any<GraphQLRequest>()).Returns(TestConstants.DictionaryResponseWithoutPaging);
        HttpClient client = BuildPagesWebApplicationFactory().CreateClient();
        string url = $"/Pages/index?mode=edit&secret={TestConstants.JssEditingSecret}&sc_itemid={TestConstants.TestItemId}&sc_version=1&sc_layoutKind=final";

        // Act
        HttpResponseMessage? response = await client.GetAsync(url);
        string? responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().NotBeNullOrEmpty();
        responseBody.Should().Contain("<code chrometype='placeholder' class='scpm' kind='open' type='text/sitecore' id='headless-main_00000000-0000-0000-0000-000000000000'></code><code chrometype='placeholder' class='scpm' kind='close' type='text/sitecore'></code></div>");
        responseBody.Should().Contain("<code chrometype='placeholder' class='scpm' kind='close' type='text/sitecore'></code></div>");
    }

    private WebApplicationFactory<TestWebApplicationProgram> BuildPagesWebApplicationFactory()
    {
        return factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSitecoreLayoutService()
                        .AddSitecorePagesHandler()
                        .AddGraphQLWithContextHandler("default", TestConstants.ContextId, siteName: TestConstants.SiteName)
                        .AsDefaultHandler();

                    services.AddSitecoreRenderingEngine(options =>
                        {
                            options.AddDefaultPartialView("_ComponentNotFound");
                        })
                        .WithSitecorePages(TestConstants.ContextId, options => { options.EditingSecret = TestConstants.JssEditingSecret; });
                });

                builder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseMiddleware<PagesRenderMiddleware>();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllerRoute(
                            name: "default",
                            pattern: "{controller=Pages}/{action=Index}");

                        endpoints.MapControllerRoute(
                            "pages-config",
                            TestConstants.ConfigRoute,
                            new { controller = "PagesSetup", action = "Config" });

                        endpoints.MapControllerRoute(
                            "pages-render",
                            TestConstants.RenderRoute,
                            new { controller = "PagesSetup", action = "Render" });
                    });
                });
            });
    }
}