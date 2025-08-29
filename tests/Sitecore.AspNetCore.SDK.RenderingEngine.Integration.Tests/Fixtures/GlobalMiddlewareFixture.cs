using System.Net;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures;

public class GlobalMiddlewareFixture : IClassFixture<TestWebApplicationFactory<TestPagesProgram>>
{
    private const string CustomHeaderName = "CustomHeader";

    private const string GlobalMiddlewareController = "UsingGlobalMiddleware";

    private readonly WebApplicationFactory<TestPagesProgram> _factory;

    private readonly MockHttpMessageHandler _mockClientHandler;

    private readonly Uri _layoutServiceUri = new("http://layout.service");

    public GlobalMiddlewareFixture(TestWebApplicationFactory<TestPagesProgram> factory)
    {
        _mockClientHandler = new MockHttpMessageHandler();
        _factory = factory
            .ConfigureServices(builder =>
            {
                builder
                    .AddSitecoreLayoutService()
                    .AddHttpHandler("mock", _ => new HttpClient(_mockClientHandler) { BaseAddress = _layoutServiceUri })
                    .AsDefaultHandler();
                builder.AddSitecoreRenderingEngine(options =>
                {
                    options.AddDefaultPartialView("_ComponentNotFound");
                    options.AddPostRenderingAction(httpContext => httpContext.Response.Headers.Append(CustomHeaderName, "value"));
                });
            })
            .Configure(app =>
            {
                app.UseSitecoreRenderingEngine();
            });
    }

    [Fact]
    public async Task HttpClient_IsInvoked()
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        HttpClient client = _factory.CreateClient();
        await client.GetAsync(GlobalMiddlewareController);

        _mockClientHandler.WasInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task Controller_ReturnsCorrectContent()
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        HttpClient client = _factory.CreateClient();
        string response = await client.GetStringAsync(GlobalMiddlewareController);

        response.Should().Be("\"success\"");
    }

    [Fact]
    public async Task HttpClient_LayoutServiceUriMapped()
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        HttpClient client = _factory.CreateClient();
        await client.GetAsync(GlobalMiddlewareController);

        _mockClientHandler.Requests.Single().RequestUri!.AbsoluteUri.Should()
            .BeEquivalentTo($"{_layoutServiceUri.AbsoluteUri}?item=%2f{GlobalMiddlewareController}");
    }

    [Fact]
    public async Task HttpClient_MissingComponent()
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithMissingComponent))
        });

        HttpClient client = _factory.CreateClient();
        string response = await client.GetStringAsync("WithRoute");

        response.Should().Contain("ComponentIsMissing");
    }

    [Fact]
    public async Task HttpClient_PostRenderingEnginActionIsExecuted()
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(GlobalMiddlewareController);

        response.Headers.Contains(CustomHeaderName).Should().BeTrue();
    }
}
