using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model.Presentation;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response.Model.Presentation;

public class RenderingFixture
{
    [Fact]
    public void Ctor_SetsDefaults()
    {
        // Arrange & act
        Rendering sut = new();

        // Assert
        sut.Id.ShouldBeNull();
        sut.InstanceId.ShouldBeNull();
        sut.PlaceholderKey.ShouldBeEmpty();
        sut.DataSource.ShouldBeNull();
        sut.Parameters.ShouldBeEmpty();
        sut.Caching.ShouldBeNull();
        sut.Personalization.ShouldBeNull();
    }
}
