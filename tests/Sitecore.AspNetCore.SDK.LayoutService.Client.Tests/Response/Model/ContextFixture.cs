using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response.Model;

public class ContextFixture
{
    [Fact]
    public void Ctor_SetDefaults()
    {
        // Arrange / Act
        Context sut = new();

        // Assert
        sut.IsEditing.ShouldBe(default);
        sut.Site.ShouldBe(default);
        sut.PageState.ShouldBeNull();
        sut.Language.ShouldBe(string.Empty);
    }
}
