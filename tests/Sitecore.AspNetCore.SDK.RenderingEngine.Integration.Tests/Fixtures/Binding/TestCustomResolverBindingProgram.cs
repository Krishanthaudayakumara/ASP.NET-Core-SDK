using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.ComponentModels;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

public class TestCustomResolverBindingProgram : IStandardTestProgram
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting()
                            .AddMvc();

                    services.AddSitecoreLayoutService()
                            .AddHttpHandler("mock", _ => new HttpClient() { BaseAddress = new Uri("http://layout.service") })
                            .AsDefaultHandler();

                    services.AddSitecoreRenderingEngine(options =>
                    {
                        options
                            .AddModelBoundView<PartialDesignDynamicPlaceholder>("PartialDesignDynamicPlaceholder")
                            .AddModelBoundView<CustomResolver>(name => name.Equals("Navigation", StringComparison.OrdinalIgnoreCase), "CustomResolver")
                            .AddDefaultComponentRenderer();
                    });
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseSitecoreRenderingEngine();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapDefaultControllerRoute();
                    });
                });
            });

    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
}
