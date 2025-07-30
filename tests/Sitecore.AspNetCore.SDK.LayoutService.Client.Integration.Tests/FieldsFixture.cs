using System.Globalization;
using Shouldly;
using Newtonsoft.Json.Linq;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Integration.Tests.MockModels;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model.Fields;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Integration.Tests;

public class FieldsFixture
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Can't be done, confuses the compiler types.")]
    public static TheoryData<ISitecoreLayoutSerializer> Serializers => new()
    {
        new JsonLayoutServiceSerializer()
    };

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Route_TextField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        TextField? routePageTitle = result?.Sitecore?.Route?.Fields["pageTitle"].Read<TextField>();
        routePageTitle!.Value.ShouldBe(jsonModel.sitecore.route.fields.pageTitle.value.Value);
        routePageTitle.EditableMarkup.ShouldBe(jsonModel.sitecore.route.fields.pageTitle.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_TextField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        Component? component = result?.Sitecore?.Route?.Placeholders["jss-main"].ComponentAt(2);
        TextField? field = component?.Fields["heading"].Read<TextField>();
        field!.Value.ShouldBe(jsonModel.sitecore.route.placeholders["jss-main"][2].fields.heading.value.Value);
        field.EditableMarkup.ShouldBe(jsonModel.sitecore.route.placeholders["jss-main"][2].fields.heading.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_HyperLinkField_UsingExternalLink_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        HyperLinkField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(26)?
            .Fields["paramsLink"]
            .Read<HyperLinkField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][26]
            .fields.paramsLink;

        resultField!.Value.ShouldNotBeNull();
        resultField.Value.Class.ShouldBe((string)expectedField.value.@class);
        resultField.Value.Target.ShouldBe((string)expectedField.value.target);
        resultField.Value.Title.ShouldBe((string)expectedField.value.title);
        resultField.Value.Href.ShouldBe((string)expectedField.value.url);
        resultField.Value.Href.ShouldBe((string)expectedField.value.href);
        resultField.Value.Text.ShouldBe((string)expectedField.value.text);

        resultField.EditableMarkupFirst.ShouldBe(expectedField.editableFirstPart.Value);
        resultField.EditableMarkupLast.ShouldBe(expectedField.editableLastPart.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_HyperLinkField_UsingInternalLink_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        HyperLinkField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(26)?
            .Fields["internalLink"]
            .Read<HyperLinkField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][26]
            .fields.internalLink;

        resultField!.Value.ShouldNotBeNull();
        resultField.Value.Class.ShouldBeNull();
        resultField.Value.Target.ShouldBeNull();
        resultField.Value.Title.ShouldBeNull();
        resultField.Value.Href.ShouldBe((string)expectedField.value.href);
        resultField.Value.Text.ShouldBeNull();

        resultField.EditableMarkupFirst.ShouldBe(expectedField.editableFirstPart.Value);
        resultField.EditableMarkupLast.ShouldBe(expectedField.editableLastPart.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_HyperLinkField_UsingEmailLink_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        HyperLinkField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(26)?
            .Fields["emailLink"]
            .Read<HyperLinkField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][26]
            .fields.emailLink;

        resultField!.Value.ShouldNotBeNull();
        resultField.Value.Class.ShouldBeNull();
        resultField.Value.Target.ShouldBeNull();
        resultField.Value.Title.ShouldBeNull();
        resultField.Value.Href.ShouldBe((string)expectedField.value.href);
        resultField.Value.Text.ShouldBe((string)expectedField.value.text);

        resultField.EditableMarkupFirst.ShouldBe(expectedField.editableFirstPart.Value);
        resultField.EditableMarkupLast.ShouldBe(expectedField.editableLastPart.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_ItemLinkField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        ItemLinkField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(29)?
            .Fields["sharedItemLink"]
            .Read<ItemLinkField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][29]
            .fields.sharedItemLink;

        resultField!.Id.ShouldBe(expectedField.id.Value);
        resultField.Url.ShouldBe(expectedField.url.Value);

        TextField? textField = resultField.Fields["textField"].Read<TextField>();
        textField!.Value.ShouldBe(expectedField.fields.textField.value.Value);
        textField.EditableMarkup.ShouldBe(expectedField.fields.textField.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_StrongTypeItemLinkField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        ItemLinkField<LinkFieldModel>? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(29)?
            .Fields["sharedItemLink"]
            .Read<ItemLinkField<LinkFieldModel>>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][29]
            .fields.sharedItemLink;

        resultField!.Id.ShouldBe(expectedField.id.Value);
        resultField.Url.ShouldBe(expectedField.url.Value);

        TextField textField = resultField.Target!.TextField;
        textField.Value.ShouldBe(expectedField.fields.textField.value.Value);
        textField.EditableMarkup.ShouldBe(expectedField.fields.textField.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_ContentListField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        ContentListField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(32)?
            .Fields["sharedContentList"]
            .Read<ContentListField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][32]
            .fields.sharedContentList;

        resultField!.Count.ShouldBe(2);
        resultField![0].Id.ShouldBe(expectedField[0].id.Value);

        TextField? firstField = resultField[0].Fields["textField"].Read<TextField>();
        firstField!.Value.ShouldBe(expectedField[0].fields.textField.value.Value);
        firstField.EditableMarkup.ShouldBe(expectedField[0].fields.textField.editable.Value);

        resultField[1].Id.ShouldBe(expectedField[1].id.Value);

        TextField? secondField = resultField[1].Fields["textField"].Read<TextField>();
        secondField!.Value.ShouldBe(expectedField[1].fields.textField.value.Value);
        secondField.EditableMarkup.ShouldBe(expectedField[1].fields.textField.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_StrongTypeContentListField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        ContentListField<LinkFieldModel>? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(32)?
            .Fields["sharedContentList"]
            .Read<ContentListField<LinkFieldModel>>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][32]
            .fields.sharedContentList;

        resultField!.Count.ShouldBe(2);
        resultField![0].Id.ShouldBe(expectedField[0].id.Value);

        TextField firstField = resultField[0].Target!.TextField;
        firstField.Value.ShouldBe(expectedField[0].fields.textField.value.Value);
        firstField.EditableMarkup.ShouldBe(expectedField[0].fields.textField.editable.Value);

        resultField[1].Id.ShouldBe(expectedField[1].id.Value);

        TextField secondField = resultField[1].Target!.TextField;
        secondField.Value.ShouldBe(expectedField[1].fields.textField.value.Value);
        secondField.EditableMarkup.ShouldBe(expectedField[1].fields.textField.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_CheckboxField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        CheckboxField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(20)?
            .Fields["checkbox"]
            .Read<CheckboxField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][20]
            .fields.checkbox;

        resultField!.Value.ShouldBe(expectedField.value.Value);
        resultField.EditableMarkup.ShouldBe(expectedField.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_DateField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        DateField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(23)?
            .Fields["dateTime"]
            .Read<DateField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][23]
            .fields.dateTime;

        resultField!.Value.ShouldBe(expectedField.value.Value);
        resultField.EditableMarkup.ShouldBe(expectedField.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_FileField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        FileField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(14)?
            .Fields["file"]
            .Read<FileField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][14]
            .fields.file;

        resultField!.Value.ShouldNotBeNull();

        resultField.Value.Description.ShouldBe((string)expectedField.value.description);
        resultField.Value.DisplayName.ShouldBe((string)expectedField.value.displayName);
        resultField.Value.Extension.ShouldBe((string)expectedField.value.extension);
        resultField.Value.Keywords.ShouldBe((string)expectedField.value.keywords);
        resultField.Value.MimeType.ShouldBe((string)expectedField.value.mimeType);
        resultField.Value.Name.ShouldBe((string)expectedField.value.name);
        resultField.Value.Size.ShouldBe(long.Parse((string)expectedField.value.size, CultureInfo.InvariantCulture));
        resultField.Value.Src.ShouldBe((string)expectedField.value.src);
        resultField.Value.Title.ShouldBe((string)expectedField.value.title);

        resultField.EditableMarkup.ShouldBe(expectedField.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_ImageField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        ImageField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(11)?
            .Fields["sample1"]
            .Read<ImageField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][11]
            .fields.sample1;

        resultField!.Value.ShouldNotBeNull();
        resultField.Value.Src.ShouldBe((string)expectedField.value.src);
        resultField.Value.Alt.ShouldBe((string)expectedField.value.alt);
        resultField.EditableMarkup.ShouldBe(expectedField.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_NumberField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        NumberField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(17)?
            .Fields["sample"]
            .Read<NumberField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][17]
            .fields.sample;
        string language = jsonModel.sitecore.context.language;

        resultField!.Value.ShouldBe(decimal.Parse(expectedField.value.Value, new CultureInfo(language)));
        resultField.EditableMarkup.ShouldBe(expectedField.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Component_RichTextField_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        RichTextField? resultField = result?.Sitecore?.Route?
            .Placeholders["jss-main"].ComponentAt(5)?
            .Placeholders["jss-styleguide-layout"].ComponentAt(2)?
            .Placeholders["jss-styleguide-section"].ComponentAt(8)?
            .Fields["sample"]
            .Read<RichTextField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["jss-main"][5]
            .placeholders["jss-styleguide-layout"][2]
            .placeholders["jss-styleguide-section"][8]
            .fields.sample;

        resultField!.Value.ShouldBe(expectedField.value.Value);
        resultField.EditableMarkup.ShouldBe(expectedField.editable.Value);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void HeadlessSxa_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/headlessSxa.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        TextField? resultField = result?.Sitecore?.Route?
            .Placeholders["headless-header"].ComponentAt(0)?
            .Placeholders["sxa-header"].ComponentAt(0)?
            .Fields["Text"]
            .Read<TextField>();

        dynamic? expectedField = jsonModel.sitecore.route
            .placeholders["headless-header"][0]
            .placeholders["sxa-header"][0]
            .fields.Text;

        resultField!.Value.ShouldBe(expectedField.value.Value);
    }
}
