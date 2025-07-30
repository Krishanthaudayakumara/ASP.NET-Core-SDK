using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using Shouldly;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Sitecore.AspNetCore.SDK.RenderingEngine.Rendering;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Tests.Rendering;

public class SitecoreRenderingContextFixture
{
    // ReSharper disable once UnusedMember.Global - Used by testing framework
    [ExcludeFromCodeCoverage]
    public static Action<IFixture> AutoSetup => f =>
    {
        f.Inject(new BindingInfo());
    };

    [Fact]
    public void Ctor_SetsDefaults()
    {
        // Arrange / Act
        SitecoreRenderingContext sut = new();

        // Assert
        sut.Response.ShouldBeNull();
        sut.Component.ShouldBeNull();
        sut.Controller.ShouldBeNull();
    }
}
