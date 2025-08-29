using System.Net;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;
using Microsoft.Extensions.Primitives;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Request;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Xunit;

// ReSharper disable StringLiteralTypo
namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures;

public class RequestMappingFixture
{
    private const string TestCookie = "ASP.NET_SessionId=Test";
    private const string TestAuthHeader = "Bearer TestToken";
    private const string MiddlewareController = "ControllerMiddleware";
    private const string QueryStringTestActionMethod = "QueryStringTest";
    private readonly Uri _layoutServiceUri = new("http://layout.service");

    [Fact]
    public async Task HttpRequest_WithValidQueryStringParams_GeneratesCorrectLayoutServiceUrl()
    {
        // Arrange
        const string testQueryString = "param1=test1&param2=test2";
        var mockClientHandler = new MockHttpMessageHandler();
        using var factory = CreateFactoryWithHandler(mockClientHandler);

        mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        HttpClient client = factory.CreateClient();

        // Act
        await client.GetAsync(MiddlewareController + "/" + QueryStringTestActionMethod + "?" + testQueryString);

        // Assert
        mockClientHandler.Requests.Single().RequestUri!.AbsoluteUri.Should()
            .BeEquivalentTo($"{_layoutServiceUri.AbsoluteUri}?item=%2f{MiddlewareController}%2f{QueryStringTestActionMethod}&{testQueryString}");
    }

    [Fact]
    public async Task HttpRequest_WithInvalidQueryStringParams_GeneratesCorrectLayoutServiceUrl()
    {
        // Arrange
        const string testQueryString = "param1=+++++++++++++++++++&param2=";
        var mockClientHandler = new MockHttpMessageHandler();
        using var factory = CreateFactoryWithHandler(mockClientHandler);

        mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        HttpClient client = factory.CreateClient();

        // Act
        await client.GetAsync(MiddlewareController + "/" + QueryStringTestActionMethod + "?" + testQueryString);

        // Assert
        mockClientHandler.Requests.Single().RequestUri!.AbsoluteUri.Should()
            .BeEquivalentTo($"{_layoutServiceUri.AbsoluteUri}?item=%2f{MiddlewareController}%2f{QueryStringTestActionMethod}");
    }

    [Fact]
    public async Task HttpRequest_WithUnencodedQueryStringParams_GeneratesCorrectLayoutServiceUrl()
    {
        // Arrange
        const string testQueryString = "param1=a b&param2=<script type=\"text/javascript\">alert(\"hello\");</script>";
        var mockClientHandler = new MockHttpMessageHandler();
        using var factory = CreateFactoryWithHandler(mockClientHandler);

        mockClientHandler.Responses.Push(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        });

        HttpClient client = factory.CreateClient();

        // Act
        await client.GetAsync(MiddlewareController + "/" + QueryStringTestActionMethod + "?" + testQueryString);

        // Assert
        mockClientHandler.Requests.Single().RequestUri!.AbsoluteUri.Should()
            .BeEquivalentTo($"{_layoutServiceUri.AbsoluteUri}?item=%2f{MiddlewareController}%2f{QueryStringTestActionMethod}&param1=a+b&param2=%3cscript+type%3d%22text%2fjavascript%22%3ealert(%22hello%22)%3b%3c%2fscript%3e");
    }

    [Fact]
    public async Task HttpRequest_WithAuthenticationHeaders_HeadersMappedToLayoutServiceRequest()
    {
        // Arrange
        var mockClientHandler = new MockHttpMessageHandler();
        using var factory = CreateFactoryWithHandler(mockClientHandler);
        
        mockClientHandler.Responses.Push(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        HttpClient client = factory.CreateClient();

        // Act
        await client.GetAsync(MiddlewareController + "/" + QueryStringTestActionMethod);

        // Assert
        mockClientHandler.Requests.Single().Headers.Authorization!.Scheme.Should().Be("Bearer");
        mockClientHandler.Requests.Single().Headers.Authorization!.Parameter.Should().Be("TestToken");
    }

    [Fact]
    public async Task HttpRequest_WithCookie_CookieMappedToLayoutServiceRequest()
    {
        // Arrange
        var mockClientHandler = new MockHttpMessageHandler();
        using var factory = CreateFactoryWithHandler(mockClientHandler);
        
        mockClientHandler.Responses.Push(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        HttpClient client = factory.CreateClient();

        // Act
        await client.GetAsync(MiddlewareController + "/" + QueryStringTestActionMethod);
        
        // Assert
        mockClientHandler.Requests.Single().Headers.TryGetValues("Cookie", out IEnumerable<string>? cookies);
        cookies.Should().NotBeNull().And.Contain(TestCookie);
    }

    private TestWebApplicationFactory<TestPagesProgram> CreateFactoryWithHandler(MockHttpMessageHandler mockClientHandler)
    {
        return new TestWebApplicationFactory<TestPagesProgram>()
            .ConfigureServices(builder =>
            {
                builder
                    .AddSitecoreLayoutService()
                    .AddHttpHandler("mock", _ => new HttpClient(mockClientHandler) { BaseAddress = _layoutServiceUri })
                    .MapFromRequest((layoutRequest, httpMessage) =>
                    {
                        if (layoutRequest.TryGetValue("Authorization", out object? auth))
                        {
                            httpMessage.Headers.Add("Authorization", auth!.ToString());
                        }

                        if (layoutRequest.TryGetValue("AspNetCookie", out object? aspnet))
                        {
                            httpMessage.Headers.Add("Cookie", aspnet!.ToString());
                        }

                        httpMessage.RequestUri = layoutRequest.BuildDefaultSitecoreLayoutRequestUri(httpMessage.RequestUri!, ["param1", "param2"]);
                    })
                    .AsDefaultHandler();

                builder.AddSitecoreRenderingEngine(options =>
                    options.MapToRequest((httpRequest, layoutRequest) =>
                    {
                        layoutRequest.Path(httpRequest.Path);
                        foreach (KeyValuePair<string, StringValues> q in httpRequest.Query)
                        {
                            layoutRequest.Add(q.Key, q.Value.ToString());
                        }

                        layoutRequest.Add("testnullvalue", null);

                        // simulate there is an authorization cookie in the HTTP request
                        httpRequest.Headers.Append("Authorization", TestAuthHeader);
                        layoutRequest.Add("Authorization", httpRequest.Headers.Authorization);

                        layoutRequest.Add("AspNetCookie", TestCookie);
                    }));
            })
            .Configure(_ => { });
    }
}
