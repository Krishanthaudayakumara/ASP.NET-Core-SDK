using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.Pages.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

public static class TestBindingExtensions
{
    // Register minimal services required by binding fixtures.
    public static IServiceCollection AddBindingTestDefaults(this IServiceCollection services)
    {
    // Ensure the layout service is registered (but not add/mark handlers here - individual fixtures should register their own handlers so they can control request behavior for tests).
    services.AddSitecoreLayoutService();

    // Register the rendering engine defaults used by binding fixtures.
    services.AddSitecoreRenderingEngine();

    return services;
    }

    // Register minimal app pipeline for binding fixtures.
    public static IApplicationBuilder UseBindingTestDefaults(this IApplicationBuilder app)
    {
        app.UseSitecoreRenderingEngine();
        return app;
    }
}
