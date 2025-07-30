using AutoFixture;
using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response.Model;

public class RouteFixture : FieldsReaderFixture<Route>
{
    // ReSharper disable once UnusedMember.Global - Used by testing framework
    public static Action<IFixture> AutoSetup => f =>
    {
        f.Behaviors.Add(new OmitOnRecursionBehavior());
    };

    [Fact]
    public void Ctor_SetsDefaults()
    {
        // Arrange / Act
        Route sut = new();

        // Assert
        sut.DatabaseName.ShouldBeNull();
        sut.DeviceId.ShouldBeNull();
        sut.ItemId.ShouldBeNull();
        sut.ItemLanguage.ShouldBeNull();
        sut.ItemVersion.ShouldBeNull();
        sut.LayoutId.ShouldBeNull();
        sut.TemplateId.ShouldBeNull();
        sut.TemplateName.ShouldBeNull();
        sut.Name.ShouldBeNull();
        sut.DisplayName.ShouldBeNull();
        sut.Placeholders.ShouldBeEmpty();
        sut.Fields.ShouldBeEmpty();
    }
}
