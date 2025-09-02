using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.CustomRenderTypes;

/// <summary>
/// Test program class for partial view renderer scenarios.
/// </summary>
public class TestPartialViewRendererProgram : IStandardTestProgram
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
                            .AddPartialView(name => name.Equals("Component-6", StringComparison.OrdinalIgnoreCase), "_PartialView")
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
