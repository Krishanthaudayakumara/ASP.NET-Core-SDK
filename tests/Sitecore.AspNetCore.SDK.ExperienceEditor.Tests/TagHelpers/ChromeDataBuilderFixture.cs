using Shouldly;
using Sitecore.AspNetCore.SDK.ExperienceEditor.TagHelpers;
using Sitecore.AspNetCore.SDK.ExperienceEditor.TagHelpers.Model;
using Xunit;

// ReSharper disable StringLiteralTypo
namespace Sitecore.AspNetCore.SDK.ExperienceEditor.Tests.TagHelpers;

public class ChromeDataBuilderFixture
{
    private readonly IChromeDataBuilder _chromeDataBuilder = new ChromeDataBuilder();

    [Fact]
    public void MapButtonToCommand_DividerEditButton_ReturnsDividerCommand()
    {
        // Arrange
        DividerEditButton button = new();

        // Act
        ChromeCommand command = _chromeDataBuilder.MapButtonToCommand(button, null, null);

        // Assert
        command.Click.ShouldBe("chrome:dummy");
        command.Icon.ShouldBe(button.Icon);
        command.IsDivider.ShouldBeTrue();
        command.Header.ShouldBe(button.Header);
        command.Tooltip.ShouldBeNull();
        command.Type.ShouldBe("separator");
    }

    [Fact]
    public void MapButtonToCommand_WebEditButtonWithClick_ReturnsChromeCommand()
    {
        // Arrange
        WebEditButton button = new()
        {
            Header = "WebEditButton",
            Icon = "/~/icon/Office/16x16/document_selection.png",
            Click = "javascript:alert(\"An edit frame button was just clicked!\")",
            Tooltip = "Doesn't do much, just a web edit button example"
        };

        // Act
        ChromeCommand command = _chromeDataBuilder.MapButtonToCommand(button, null, null);

        // Assert
        command.Click.ShouldBe(button.Click);
        command.Icon.ShouldBe(button.Icon);
        command.IsDivider.ShouldBeFalse();
        command.Header.ShouldBe(button.Header);
        command.Tooltip.ShouldBe(button.Tooltip);
        command.Type.ShouldBe(button.Type);
    }

    [Fact]
    public void MapButtonToCommand_FieldEditButtonWithFieldsAndItemId_ReturnsChromeCommand()
    {
        // Arrange
        FieldEditButton button = new()
        {
            Header = "FieldEditButton",
            Icon = "/~/icon/Office/16x16/pencil.png",
            Fields = ["applyRedToText", "sampleList"],
            Tooltip = "Allows you to open field editor for specified fields"
        };
        string itemId = Guid.NewGuid().ToString();

        // Act
        ChromeCommand command = _chromeDataBuilder.MapButtonToCommand(button, itemId, null);

        // Assert
        command.Click.ShouldBe($"javascript:Sitecore.PageModes.PageEditor.postRequest('webedit:fieldeditor(command={DefaultEditFrameButtonIds.Edit}, fields={string.Join('|', button.Fields)}, id={itemId})',null,false)");
        command.Icon.ShouldBe(button.Icon);
        command.IsDivider.ShouldBeFalse();
        command.Header.ShouldBe(button.Header);
        command.Tooltip.ShouldBe(button.Tooltip);
        command.Type.ShouldBeNull();
    }

    [Fact]
    public void MapButtonToCommand_WebEditButtonWithInvalidClick_ReturnsChromeCommand()
    {
        // Arrange
        WebEditButton button = new()
        {
            Header = "WebEditButton",
            Icon = "/~/icon/Office/16x16/document_selection.png",
            Click = "test:alert(\"Missing parentheses!\"",
            Tooltip = "Doesn't do much, just a web edit button example"
        };
        string itemId = Guid.NewGuid().ToString();

        // Act
        Action action = () => _chromeDataBuilder.MapButtonToCommand(button, itemId, null);

        // Assert
        action.Should().Throw<ArgumentException>("Message with arguments must end with ).");
    }

    [Fact]
    public void MapButtonToCommand_WebEditButtonWithParameters_ReturnsChromeCommand()
    {
        // Arrange
        WebEditButton button = new()
        {
            Header = "WebEditButton",
            Icon = "/~/icon/Office/16x16/document_selection.png",
            Click = "test:MethodWithParams(\"\")",
            Tooltip = "Doesn't do much, just a web edit button example",
            Parameters = new Dictionary<string, object?>
            {
                { "btnParam1", "btnVal1" },
                { "btnParam2", "btnVal2" },
            }
        };
        string itemId = Guid.NewGuid().ToString();
        Dictionary<string, object?> frameParams = new()
        {
            {
                "frameParam1", "frameVal1"
            },
            {
                "frameParam2", "frameVal2"
            },
        };

        // Act
        ChromeCommand command = _chromeDataBuilder.MapButtonToCommand(button, itemId, frameParams);

        // Assert
        command.Click.ShouldBe($"javascript:Sitecore.PageModes.PageEditor.postRequest('test:MethodWithParams(\"\"=, id={itemId}, btnParam1=btnVal1, btnParam2=btnVal2, frameParam1=frameVal1, frameParam2=frameVal2)',null,false)");
        command.Icon.ShouldBe(button.Icon);
        command.IsDivider.ShouldBeFalse();
        command.Header.ShouldBe(button.Header);
        command.Tooltip.ShouldBe(button.Tooltip);
        command.Type.ShouldBe(button.Type);
    }
}
