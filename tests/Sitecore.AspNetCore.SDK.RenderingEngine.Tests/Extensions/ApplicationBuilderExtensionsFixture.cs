using System.Reflection;
using System.Reflection.PortableExecutable;
using Shouldly;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.RenderingEngine.Configuration;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Localization;
using Sitecore.AspNetCore.SDK.RenderingEngine.Middleware;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Tests.Extensions;

public class ApplicationBuilderExtensionsFixture
{
    [Fact]
    public void UseSitecoreRenderingEngine_NullApplicationBuilder_Throws()
    {
        // Arrange
        Func<IApplicationBuilder> act =
            () => RenderingEngine.Extensions.ApplicationBuilderExtensions.UseSitecoreRenderingEngine(null!);

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public void UseSitecoreRenderingEngine_WithAppBuilderAndRenderingEngineServicesAdded_CallsMiddleware(ServiceCollection services, IApplicationBuilder appBuilder)
    {
        // Arrange
        services.AddSitecoreRenderingEngine();
        appBuilder.ApplicationServices.GetService(typeof(SitecoreQueryStringCultureProvider))
            .Returns(new SitecoreQueryStringCultureProvider());
        IOptions<RequestLocalizationOptions>? options = Substitute.For<IOptions<RequestLocalizationOptions>>();
        appBuilder.ApplicationServices.GetService(typeof(IOptions<RequestLocalizationOptions>)).Returns(options);
        options.Value.Returns(new RequestLocalizationOptions());

        // Act
        appBuilder.UseSitecoreRenderingEngine();

        // Assert
        bool received = appBuilder.ReceivedCalls().Any(c => c.GetArguments().OfType<Delegate>().Any(d =>
            d.Target?.GetType().GetField("_middleware", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(d.Target).As<Type>().FullName == typeof(RenderingEngineMiddleware).FullName));
        received.ShouldBeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    public void UseSitecoreRenderingEngine_WithAppBuilderAndWithoutRenderingEngineServices_Throws(IApplicationBuilder appBuilder)
    {
        // Arrange
        appBuilder.ApplicationServices.GetService(typeof(RenderingEngineMarkerService)).Returns(null);
        Func<IApplicationBuilder> act =
            appBuilder.UseSitecoreRenderingEngine;

        // Act & Assert
        var ex = Should.Throw<InvalidOperationException>(() => act()); // TODO: Assert exception properties manually// TODO: Assert exception.Message manually extension method in Startup.ConfigureServices?");
    }

    [Fact]
    public void ApplicationBuilderExtensions_AddRenderingEngineMapping_Guarded()
    {
        Action act =
            () => RenderingEngine.Extensions.ApplicationBuilderExtensions.AddRenderingEngineMapping(null!, (_, _) => { });

        var ex = Should.Throw<NullReferenceException>(() => act()); // TODO: Assert exception properties manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public void ApplicationBuilderExtensions_AddRenderingEngineMapping_Without_MappingAction_Guarded(IApplicationBuilder app)
    {
        // Arrange
        app.ApplicationServices.GetService(typeof(IOptions<RenderingEngineOptions>)).Returns(Substitute.For<IOptions<RenderingEngineOptions>>());
        Action act =
            () => app.AddRenderingEngineMapping(null!);

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually;
    }
}
