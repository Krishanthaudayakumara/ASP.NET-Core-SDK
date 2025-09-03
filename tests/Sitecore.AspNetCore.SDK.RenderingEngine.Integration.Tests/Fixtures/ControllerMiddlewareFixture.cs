using System.Net;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures;

public class ControllerMiddlewareFixture : IClassFixture<TestWebApplicationFactory<TestBasicProgram>>
{
    private const string MiddlewareController = "ControllerMiddleware";

    private const string GlobalMiddlewareController = "GlobalMiddleware";

    private readonly TestWebApplicationFactory<TestBasicProgram> _factory;

    public ControllerMiddlewareFixture(TestWebApplicationFactory<TestBasicProgram> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HttpClient_IsInvoked()
    {
        // Reset mock state before test
        _factory.MockClientHandler.Requests.Clear();

        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        HttpClient client = _factory.CreateClient();
        await client.GetAsync(MiddlewareController);

        _factory.MockClientHandler.WasInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task HttpClient_IsNotInvoked()
    {
        // Create a fresh client to avoid shared state from previous tests
        // Clear any previous requests but keep the factory's handler
        _factory.MockClientHandler.Requests.Clear();
        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        // Use a fresh client to avoid middleware state conflicts
        HttpClient client = _factory.CreateClient();
        await client.GetAsync(GlobalMiddlewareController);

        _factory.MockClientHandler.WasInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task Controller_ReturnsCorrectContent()
    {
        // Reset mock state before test
        _factory.MockClientHandler.Requests.Clear();
        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        HttpClient client = _factory.CreateClient();
        string response = await client.GetStringAsync(GlobalMiddlewareController);

        response.Should().Be("\"success\"");
    }

    [Fact]
    public async Task HttpClient_LayoutServiceUriMapped()
    {
        // Reset mock state before test
        _factory.MockClientHandler.Requests.Clear();
        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        HttpClient client = _factory.CreateClient();
        await client.GetAsync(MiddlewareController);

        _factory.MockClientHandler.Requests.Single().RequestUri!.AbsoluteUri.Should()
            .BeEquivalentTo($"{_factory.LayoutServiceUri.AbsoluteUri}?item=%2f{MiddlewareController}");
    }
}