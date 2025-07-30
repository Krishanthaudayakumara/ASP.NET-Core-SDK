using Shouldly;
using Newtonsoft.Json.Linq;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Integration.Tests;

public class ContextFixture
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Can't be done, confuses the compiler types.")]
    public static TheoryData<ISitecoreLayoutSerializer> Serializers => new()
    {
        new JsonLayoutServiceSerializer()
    };

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Context_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        Context? resultContext = result?.Sitecore?.Context;

        dynamic? expectedContext = jsonModel.sitecore.context;

        resultContext.ShouldNotBeNull();
        resultContext!.IsEditing.ShouldBe((bool)expectedContext.pageEditing);

        resultContext.Site.ShouldNotBeNull();
        resultContext.Site!.Name.ShouldBe((string)expectedContext.site.name);
        resultContext.PageState.ShouldBe((PageState)expectedContext.pageState);
        resultContext.Language.ShouldBe((string)expectedContext.language);
    }
}
