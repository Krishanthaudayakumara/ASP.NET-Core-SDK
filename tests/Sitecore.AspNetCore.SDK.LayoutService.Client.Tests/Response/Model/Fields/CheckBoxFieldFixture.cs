using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model.Fields;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response.Model.Fields;

public class CheckBoxFieldFixture : FieldFixture<CheckboxField>
{
    [Fact]
    public void Ctor_SetsDefaults()
    {
        // Arrange / Act
        CheckboxField instance = new();

        // Assert
        instance.Value.ShouldBe(default);
        instance.EditableMarkup.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Ctor_WithValue_SetsValue(bool value)
    {
        // Arrange / Act
        CheckboxField instance = new(value);

        // Assert
        instance.Value.ShouldBe(value);
        instance.EditableMarkup.ShouldBeEmpty();
    }
}
