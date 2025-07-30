using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response.Model;

public class EditableChromeFixture
{
    [Fact]
    public void Ctor_SetsDefaults()
    {
        // Act
        EditableChrome sut = new();

        // Assert
        sut.Name.ShouldBe("code");
        sut.Type.ShouldBe("text/sitecore");
        sut.Content.ShouldBeEmpty();
        sut.Attributes.ShouldBeEmpty();
    }
}
