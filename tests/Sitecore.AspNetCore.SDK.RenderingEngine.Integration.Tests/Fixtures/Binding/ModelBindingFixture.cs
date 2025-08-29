using System.Net;
using AwesomeAssertions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

public class ModelBindingFixture(TestWebApplicationFactory<TestModelBindingProgram> factory) : IClassFixture<TestWebApplicationFactory<TestModelBindingProgram>>
{
    private readonly TestWebApplicationFactory<TestModelBindingProgram> _factory = factory;

    [Fact]
    public async Task SitecoreRouteModelBinding_ReturnsCorrectData()
    {
        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithNestedPlaceholder))
        });

        HttpClient client = _factory.CreateClient();
        string response = await client.GetStringAsync("WithBoundSitecoreRoute");

        // assert that the SitecoreRouteProperty attribute binding worked
        response.Should().Contain(TestConstants.DatabaseName);

        // assert that the SitecoreRouteField attribute binding worked
        response.Should().Contain(TestConstants.PageTitle);

        // assert that the SitecoreRoute model binding worked
        response.Should().Contain(TestConstants.TestItemId);
    }

    [Fact]
    public async Task SitecoreContextModelBinding_ReturnsCorrectData()
    {
        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithNestedPlaceholder))
        });

        HttpClient client = _factory.CreateClient();
        string response = await client.GetStringAsync("WithBoundSitecoreContext");

        // assert that the SitecoreContextProperty attribute binding worked
        response.Should().Contain(TestConstants.Language);
        response.Should().Contain("False");

        // assert that the SitecoreContext model binding worked
        response.Should().Contain(PageState.Normal.ToString());
    }

    [Fact]
    public async Task SitecoreResponseModelBinding_ReturnsCorrectData()
    {
        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(Serializer.Serialize(CannedResponses.WithNestedPlaceholder))
        });

        HttpClient client = _factory.CreateClient();
        string response = await client.GetStringAsync("WithBoundSitecoreResponse");

        // assert that the SitecoreLayoutResponse attribute binding worked
        response.Should().Contain(TestConstants.DatabaseName);
    }
}