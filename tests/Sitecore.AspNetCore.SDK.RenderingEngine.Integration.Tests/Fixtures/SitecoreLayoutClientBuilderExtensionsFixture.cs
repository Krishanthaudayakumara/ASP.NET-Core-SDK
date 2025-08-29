using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Configuration;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Interfaces;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures;

public class SitecoreLayoutClientBuilderExtensionsFixture : IClassFixture<TestWebApplicationFactory<TestPagesProgram>>
{
    private readonly MockHttpMessageHandler _messageHandler;
    private readonly TestWebApplicationFactory<TestPagesProgram> _factory;

    public SitecoreLayoutClientBuilderExtensionsFixture(TestWebApplicationFactory<TestPagesProgram> factory)
    {
        _factory = factory;
        _messageHandler = new MockHttpMessageHandler();
        _factory
            .ConfigureServices(builder =>
            {
                ISitecoreLayoutClientBuilder lsc = builder
                    .AddSitecoreLayoutService();

                lsc.AddHttpHandler("mock", _ => new HttpClient(_messageHandler) { BaseAddress = new Uri("http://layout.service") });

                lsc.AddHttpHandler("otherMock", _ => new HttpClient(_messageHandler) { BaseAddress = new Uri("http://layout.service") })
                    .AsDefaultHandler();
            })
            .Configure(app =>
            {
                app.UseSitecoreRenderingEngine();
            });
    }

    [Fact]
    public void DefaultHandler_SetsSitecoreLayoutServiceOptions()
    {
        // Act
        IOptions<SitecoreLayoutClientOptions> layoutService = _factory.Services.GetRequiredService<IOptions<SitecoreLayoutClientOptions>>();

        // Assert
        layoutService.Value.DefaultHandler.Should().Be("otherMock");
    }
}
