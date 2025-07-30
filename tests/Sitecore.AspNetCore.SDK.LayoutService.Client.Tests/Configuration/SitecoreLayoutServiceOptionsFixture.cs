using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Configuration;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Configuration;

public class SitecoreLayoutServiceOptionsFixture
{
    [Fact]
    public void Ctor_SetsDefaults()
    {
        // Arrange / Act
        SitecoreLayoutClientOptions sut = new();

        // Assert
        sut.DefaultHandler.ShouldBeNull();
        sut.HandlerRegistry.ShouldBeEmpty();
    }
}
