using System.Net;
using Shouldly;
using Microsoft.AspNetCore.TestHost;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures;

public class GlobalMiddlewareFixture : IDisposable
{
    private const string CustomHeaderName = "CustomHeader";

    private const string GlobalMiddlewareController = "UsingGlobalMiddleware";

    private readonly TestServer _server;

    private readonly MockHttpMessageHandler _mockClientHandler;

    private readonly Uri _layoutServiceUri = new("http://layout.service");

    public GlobalMiddlewareFixture()
    {
        TestServerBuilder testHostBuilder = new();
        _mockClientHandler = new MockHttpMessageHandler();
        testHostBuilder
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

        _server = testHostBuilder.BuildServer(new Uri("http://localhost"));
    }

    [Fact]
    public async Task HttpClient_IsInvoked()
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        HttpClient client = _server.CreateClient();
        await client.GetAsync(GlobalMiddlewareController);

        _mockClientHandler.WasInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Controller_ReturnsCorrectContent()
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        HttpClient client = _server.CreateClient();
        string response = await client.GetStringAsync(GlobalMiddlewareController);

        response.ShouldBe("\"success\"");
    }

    [Fact]
    public async Task HttpClient_LayoutServiceUriMapped()
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        HttpClient client = _server.CreateClient();
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

        HttpClient client = _server.CreateClient();
        string response = await client.GetStringAsync("WithRoute");

        response.ShouldContain("ComponentIsMissing");
    }

    [Fact]
    public async Task HttpClient_PostRenderingEnginActionIsExecuted()
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        HttpClient client = _server.CreateClient();
        HttpResponseMessage response = await client.GetAsync(GlobalMiddlewareController);

        response.Headers.Contains(CustomHeaderName).ShouldBeTrue();
    }

    public void Dispose()
    {
        _server.Dispose();
        _mockClientHandler.Dispose();
        GC.SuppressFinalize(this);
    }
}
