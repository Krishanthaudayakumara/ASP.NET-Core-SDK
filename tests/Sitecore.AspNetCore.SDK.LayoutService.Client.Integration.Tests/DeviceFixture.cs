using Shouldly;
using Newtonsoft.Json.Linq;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model.Presentation;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Integration.Tests;

public class DeviceFixture
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Can't be done, confuses the compiler types.")]
    public static TheoryData<ISitecoreLayoutSerializer> Serializers => new()
    {
        new JsonLayoutServiceSerializer()
    };

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Device_CanBeRead(ISitecoreLayoutSerializer serializer)
    {
        // Arrange
        string json = File.ReadAllText("./Json/edit.json");
        dynamic jsonModel = JObject.Parse(json);

        // Act
        SitecoreLayoutResponseContent? result = serializer.Deserialize(json);

        // Assert
        Device resultDevice = result!.Sitecore!.Devices[0];

        dynamic? expectedDevice = jsonModel.sitecore.devices[0];

        resultDevice.ShouldNotBeNull();
        resultDevice.Id.ShouldBe((string)expectedDevice.id);
        resultDevice.LayoutId.ShouldBe((string)expectedDevice.layoutId);
        resultDevice.Placeholders.ShouldBeEmpty();
        resultDevice.Renderings.Count.ShouldBe(3);

        for (int i = 0; i < 3; i++)
        {
            resultDevice.Renderings[i].Id.ShouldBe((string)expectedDevice.renderings[i].id);
            resultDevice.Renderings[i].InstanceId.ShouldBe((string)expectedDevice.renderings[i].instanceId);
            resultDevice.Renderings[i].PlaceholderKey.ShouldBe((string)expectedDevice.renderings[i].placeholderKey);
            resultDevice.Renderings[i].Parameters.ShouldBeEmpty();
            resultDevice.Renderings[i].Caching!.Cacheable.ShouldBe(null);
            resultDevice.Renderings[i].Caching!.ClearOnIndexUpdate.ShouldBe(null);
            resultDevice.Renderings[i].Caching!.VaryByData.ShouldBe(null);
            resultDevice.Renderings[i].Caching!.VaryByDevice.ShouldBe(null);
            resultDevice.Renderings[i].Caching!.VaryByLogin.ShouldBe(null);
            resultDevice.Renderings[i].Caching!.VaryByParameters.ShouldBe(null);
            resultDevice.Renderings[i].Caching!.VaryByQueryString.ShouldBe(null);
            resultDevice.Renderings[i].Caching!.VaryByUser.ShouldBe(null);
            resultDevice.Renderings[i].Personalization!.Conditions.ShouldBe(null);
            resultDevice.Renderings[i].Personalization!.MultiVariateTestId.ShouldBe(null);
            resultDevice.Renderings[i].Personalization!.PersonalizationTest.ShouldBe(null);
            resultDevice.Renderings[i].Personalization!.Rules.ShouldBe(null);
        }
    }
}
