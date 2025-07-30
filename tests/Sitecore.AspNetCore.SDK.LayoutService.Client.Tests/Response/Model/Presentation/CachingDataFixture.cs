using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model.Presentation;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response.Model.Presentation;

public class CachingDataFixture
{
    [Fact]
    public void Ctor_SetsDefaults()
    {
        // Arrange & act
        CachingData sut = new();

        // Assert
        sut.Cacheable.ShouldBeNull();
        sut.ClearOnIndexUpdate.ShouldBeNull();
        sut.VaryByData.ShouldBeNull();
        sut.VaryByDevice.ShouldBeNull();
        sut.VaryByLogin.ShouldBeNull();
        sut.VaryByParameters.ShouldBeNull();
        sut.VaryByQueryString.ShouldBeNull();
        sut.VaryByUser.ShouldBeNull();
    }
}
