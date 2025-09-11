using System.Net;
using AwesomeAssertions;
using NSubstitute;
using Sitecore.AspNetCore.SDK.GraphQL.Request;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Sitecore.AspNetCore.SDK.Pages.Request.Handlers.GraphQL;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

public class ModelBindingFixture(TestWebApplicationFactory<TestBindingProgram> factory) : IClassFixture<TestWebApplicationFactory<TestBindingProgram>>
{
    private readonly TestWebApplicationFactory<TestBindingProgram> _factory = factory;

    [Fact]
    public async Task SitecoreRouteModelBinding_ReturnsCorrectData()
    {
        _factory.MockGraphQLClient.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLHttpRequestWithHeaders>())
            .Returns(TestConstants.SimpleEditingLayoutQueryResponse);

        HttpClient client = _factory.CreateClient();
        string url = "/Pages/WithBoundSitecoreRoute";

        string response = await client.GetStringAsync(url);

        response.Should().Contain(TestConstants.DatabaseName);
        response.Should().Contain(TestConstants.PageTitle);
        response.Should().Contain(TestConstants.TestItemId);
    }

    [Fact]
    public async Task SitecoreContextModelBinding_ReturnsCorrectData()
    {
        _factory.MockGraphQLClient.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLHttpRequestWithHeaders>())
            .Returns(TestConstants.SimpleEditingLayoutQueryResponse);

        HttpClient client = _factory.CreateClient();
        string url = "/Pages/WithBoundSitecoreContext";

        string response = await client.GetStringAsync(url);

        response.Should().Contain(TestConstants.Language);
        response.Should().Contain("False");
        response.Should().Contain(PageState.Normal.ToString());
    }

    [Fact]
    public async Task SitecoreResponseModelBinding_ReturnsCorrectData()
    {
        _factory.MockGraphQLClient.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLHttpRequestWithHeaders>())
            .Returns(TestConstants.SimpleEditingLayoutQueryResponse);

        HttpClient client = _factory.CreateClient();
        string url = "/Pages/WithBoundSitecoreResponse";

        string response = await client.GetStringAsync(url);

        response.Should().Contain(TestConstants.DatabaseName);
    }
}