using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.ComponentModels;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.CustomRenderTypes;

public class TestMultipleComponentsAddedProgram : IStandardTestProgram
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
                            .AddModelBoundView<Component3>(name => name.Equals("Component-3", StringComparison.OrdinalIgnoreCase), "Component3")
                            .AddModelBoundView<Component6>(name => name.Equals("Component-6", StringComparison.OrdinalIgnoreCase), "Component6")
                            .AddDefaultPartialView("_ComponentNotFound")
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
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddSitecoreLayoutService()
            .AddHttpHandler("mock", _ => new HttpClient())
            .AsDefaultHandler();

        builder.Services.AddSitecoreRenderingEngine(options =>
        {
            options
                .AddModelBoundView<Component3>(name => name.Equals("Component-3", StringComparison.OrdinalIgnoreCase), "Component3")
                .AddModelBoundView<Component6>(name => name.Equals("Component-6", StringComparison.OrdinalIgnoreCase), "Component6")
                .AddDefaultComponentRenderer();
        });

        WebApplication app = builder.Build();

        app.UseRouting();
        app.UseSitecoreRenderingEngine();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });

        app.Run();
    }
}
