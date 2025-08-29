using System.Globalization;
using System.Net;
using AwesomeAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.TagHelpers;

public class DateFieldTagHelperFixture : IClassFixture<TestWebApplicationFactory<TestPagesProgram>>
{
    private readonly WebApplicationFactory<TestPagesProgram> _factory;
    private readonly MockHttpMessageHandler _mockClientHandler;
    private readonly Uri _layoutServiceUri = new("http://layout.service");

    public DateFieldTagHelperFixture(TestWebApplicationFactory<TestPagesProgram> factory)
    {
        _factory = factory;
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
                    options
                        .AddModelBoundView<ComponentModels.ComponentWithDates>("Component-With-Dates", "ComponentWithDates")
                        .AddDefaultComponentRenderer();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseSitecoreRenderingEngine();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapDefaultControllerRoute();
                });
            });}

    [Fact]
    public async Task DateTagHelper_DoesNotResetOtherTagHelperOutput()
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
        HtmlNode? sectionNode = doc.DocumentNode.ChildNodes.First(n => n.HasClass("component-with-dates"));
        HtmlNode? text = sectionNode.ChildNodes[7];

        // Assert
        // check scenario that DateTagHelper does not reset values of nested helpers.
        text.InnerHtml.Should().Contain(TestConstants.TestFieldValue);
    }

    [Fact]
    public async Task DateTagHelper_GeneratesProperDate()
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
        HtmlNode? sectionNode = doc.DocumentNode.ChildNodes.First(n => n.HasClass("component-with-dates"));

        // Assert
        sectionNode.ChildNodes[1].InnerHtml.Should().Be("05/04/2012");
        sectionNode.ChildNodes[3].InnerHtml.Should().Be("05/04/2012 00:00:00");
        sectionNode.ChildNodes[5].InnerHtml.Should().Be(TestConstants.DateTimeValue.ToString(CultureInfo.CurrentCulture));
        sectionNode.ChildNodes[9].InnerHtml.Should().Contain("04.05.2012");
    }
}
