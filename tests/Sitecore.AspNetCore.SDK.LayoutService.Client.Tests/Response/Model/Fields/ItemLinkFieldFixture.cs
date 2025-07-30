using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model.Fields;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response.Model.Fields;

public class ItemLinkFieldFixture : FieldFixture<ItemLinkField>
{
    [Fact]
    public void Ctor_SetsDefaults()
    {
        // Arrange / Act
        ItemLinkField instance = new();

        // Assert
        instance.Fields.ShouldBeEmpty();
        instance.Id.ShouldBe(default(Guid));
        instance.Url.ShouldBeNull();
    }
}
