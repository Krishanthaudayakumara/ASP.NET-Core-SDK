using System.Net;
using AwesomeAssertions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.TestHost;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Mocks;
using Sitecore.AspNetCore.SDK.Tracking;
using Sitecore.AspNetCore.SDK.Tracking.VisitorIdentification;
using Xunit;

// ReSharper disable StringLiteralTypo
namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Tracking;

public class TrackingProxyFixture : IClassFixture<TestWebApplicationFactory<TestTrackingProgram>>, IDisposable
{
    private readonly TestWebApplicationFactory<TestTrackingProgram> _factory;

    public TrackingProxyFixture(TestWebApplicationFactory<TestTrackingProgram> factory)
    {
        _factory = factory;

        // Clear any previous state and add response for this test
        _factory.MockClientHandler.Requests.Clear();
        _factory.MockClientHandler.Responses.Clear();
        _factory.MockClientHandler.Responses.Push(new HttpResponseMessage(HttpStatusCode.OK));
    }

    [Fact]
    public async Task SitecoreRequests_ToLayouts_MustBeProxied()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        HttpRequestMessage request = new(HttpMethod.Get, new Uri("/layouts/System/VisitorIdentification.js", UriKind.Relative));
        request.Headers.Add("Cookie", ["ASP.NET_SessionId=rku2oxmotbrkwkfxe0cpfrvn; path=/; HttpOnly; SameSite=Lax", "SC_ANALYTICS_GLOBAL_COOKIE=0f82f53555ce4304a1ee8ae99ab9f9a8|False; expires = Fri, 15 - Mar - 2030 13:15:08 GMT; path =/; HttpOnly"]);
        request.Headers.Add("X-Forwarded-For", "172.217.16.14");

        // Act
        await client.SendAsync(request);

        // Asserts
        _factory.MockClientHandler.Requests.Should().ContainSingle("A call to rendering middleware is not expected.");
        _factory.MockClientHandler.Requests[0].RequestUri!.Host.Should().Be(_factory.LayoutServiceUri.Host);
        _factory.MockClientHandler.Requests[0].RequestUri!.Scheme.Should().Be(_factory.LayoutServiceUri.Scheme);
        _factory.MockClientHandler.Requests[0].RequestUri!.PathAndQuery.Should().Be("/layouts/System/VisitorIdentification.js");
        _factory.MockClientHandler.Requests[0].Headers.Should().Contain(h => h.Key.Equals("Cookie"));
        _factory.MockClientHandler.Requests[0].Headers.GetValues("x-forwarded-for").First().ToUpperInvariant().Should().Be("172.217.16.14");
        _factory.MockClientHandler.Requests[0].Headers.GetValues("x-forwarded-host").First().ToUpperInvariant().Should().Be("LOCALHOST");
        _factory.MockClientHandler.Requests[0].Headers.GetValues("x-forwarded-proto").First().ToUpperInvariant().Should().Be("HTTP");
    }

    public void Dispose()
    {
        _factory.MockClientHandler.Dispose();
        GC.SuppressFinalize(this);
    }
}