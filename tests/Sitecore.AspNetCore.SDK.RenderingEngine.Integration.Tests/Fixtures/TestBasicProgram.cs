using Microsoft.AspNetCore.Hosting;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures;

/// <summary>
/// Basic test program class for controller middleware integration tests.
/// Does not add global rendering engine middleware to avoid conflicts with attribute-based middleware.
/// </summary>
public class TestBasicProgram : IStandardTestProgram
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

                    // Use the same configuration as GlobalMiddlewareFixture which works correctly
                    services.AddSitecoreRenderingEngine(options =>
                    {
                        options.AddDefaultPartialView("_ComponentNotFound");
                        options.AddDefaultComponentRenderer();
                    });
                })
                .Configure(app =>
                {
                    app.UseRouting();

                    // Add global rendering engine middleware to enable route-to-item mapping
                    app.UseSitecoreRenderingEngine();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapDefaultControllerRoute();
                    });
                });
            });
}
