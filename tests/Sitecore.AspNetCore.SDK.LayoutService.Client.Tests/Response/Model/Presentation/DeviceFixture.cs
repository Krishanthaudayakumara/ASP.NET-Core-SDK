using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model.Presentation;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response.Model.Presentation;

public class DeviceFixture
{
    [Fact]
    public void Ctor_SetsDefaults()
    {
        // Arrange & act
        Device sut = new();

        // Assert
        sut.Id.ShouldBeNull();
        sut.LayoutId.ShouldBeNull();
        sut.Placeholders.ShouldBeEmpty();
        sut.Renderings.ShouldBeEmpty();
    }
}
