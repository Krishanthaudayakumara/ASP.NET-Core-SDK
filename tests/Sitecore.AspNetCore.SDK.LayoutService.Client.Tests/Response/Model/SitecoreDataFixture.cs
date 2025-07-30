using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response.Model;

public class SitecoreDataFixture
{
    [Fact]
    public void Ctor_PropertiesAreDefaults()
    {
        // Arrange
        SitecoreData sut = new();

        // Act / Assert
        sut.Context.ShouldBe(default);
        sut.Route.ShouldBe(default);
        sut.Devices.ShouldBeEmpty();
    }
}
