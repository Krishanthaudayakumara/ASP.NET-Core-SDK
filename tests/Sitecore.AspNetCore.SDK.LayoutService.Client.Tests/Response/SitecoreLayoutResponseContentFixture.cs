using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response;

public class SitecoreLayoutResponseContentFixture
{
    [Fact]
    public void Ctor_PropertiesAreDefaults()
    {
        // Arrange
        SitecoreLayoutResponseContent sut = new();

        // Act / Assert
        sut.Sitecore.ShouldBe(default);
    }
}
