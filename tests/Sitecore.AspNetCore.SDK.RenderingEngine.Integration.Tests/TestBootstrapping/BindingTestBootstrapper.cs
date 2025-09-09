using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sitecore.AspNetCore.SDK.RenderingEngine.Configuration;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.TestBootstrapping;

public class BindingTestBootstrapper : ITestBootstrapper
{
    private readonly Action<RenderingEngineOptions>? _configureOptions;
    private readonly Action<IApplicationBuilder>? _configureApp;

    public BindingTestBootstrapper(Action<RenderingEngineOptions>? configureOptions = null, Action<IApplicationBuilder>? configureApp = null)
    {
        _configureOptions = configureOptions;
        _configureApp = configureApp;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSitecoreRenderingEngine();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseSitecoreRenderingEngine();

        if (_configureOptions is not null)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<RenderingEngineOptions>>().Value;
            _configureOptions(options);
        }

        _configureApp?.Invoke(app);
    }
}
