using Shouldly;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.AspNetCore.SDK.RenderingEngine.Binding;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Interfaces;
using Sitecore.AspNetCore.SDK.RenderingEngine.Localization;
using Sitecore.AspNetCore.SDK.RenderingEngine.Rendering;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Tests.Extensions;

public class ServiceCollectionExtensionsFixture
{
    [Fact]
    public void AddSitecoreRenderingEngine_IsGuarded()
    {
        Func<ISitecoreRenderingEngineBuilder> act =
            () => ServiceCollectionExtensions.AddSitecoreRenderingEngine(null!);

        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually;
    }

    [Fact]
    public void AddSitecoreRenderingEngine_ServiceCollection_Contains_ExpectedServices()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        services.AddSitecoreRenderingEngine();

        // Assert
        services.ShouldContain(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IHttpContextAccessor) && serviceDescriptor.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ISitecoreLayoutRequestMapper) && serviceDescriptor.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IComponentRendererFactory) && serviceDescriptor.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IEditableChromeRenderer) && serviceDescriptor.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IViewModelBinder) && serviceDescriptor.Lifetime == ServiceLifetime.Scoped);
        services.ShouldContain(serviceDescriptor => serviceDescriptor.ServiceType == typeof(SitecoreQueryStringCultureProvider) && serviceDescriptor.Lifetime == ServiceLifetime.Singleton);
    }
}
