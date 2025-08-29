using System.Net;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Mocks;
using Sitecore.AspNetCore.SDK.SearchOptimization.Extensions;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.SearchOptimization;

public class SitemapProxyFixture : IClassFixture<TestWebApplicationFactory<TestPagesProgram>>
{
    private readonly TestWebApplicationFactory<TestPagesProgram> _factory;
    private readonly MockHttpMessageHandler _mockClientHandler = new();
    private readonly Uri _cdInstanceUri = new("http://cd");

    public SitemapProxyFixture(TestWebApplicationFactory<TestPagesProgram> factory)
    {
        _mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        _factory = factory;
        _ = _factory.ConfigureServices(builder =>
            {
                builder.AddSingleton<IHttpClientFactory>(_ =>
                {
                    return new CustomHttpClientFactory(
                        () =>
                            new HttpClient(_mockClientHandler));
                });

                builder.AddSitemap(c => c.Url = _cdInstanceUri);
            })
            .Configure(app =>
            {
                app.UseSitemap();
            });}

    [Fact]
    public async Task SitemapRequest_MustBeProxied()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        HttpRequestMessage request = new(HttpMethod.Get, new Uri("/sitemap.xml", UriKind.Relative));

        // Act
        await client.SendAsync(request);

        // Asserts
        _mockClientHandler.Requests.Should().ContainSingle();
        _mockClientHandler.Requests[0].RequestUri!.Host.Should().Be(_cdInstanceUri.Host);
        _mockClientHandler.Requests[0].RequestUri!.Scheme.Should().Be(_cdInstanceUri.Scheme);
        _mockClientHandler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/sitemap.xml");
    }
}

