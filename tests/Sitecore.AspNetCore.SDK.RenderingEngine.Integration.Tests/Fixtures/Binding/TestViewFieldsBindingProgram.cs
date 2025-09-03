using Microsoft.AspNetCore.Hosting;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;
using Sitecore.AspNetCore.SDK.TestData;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

/// <summary>
/// Test program class for view fields binding scenarios.
/// </summary>
public class TestViewFieldsBindingProgram : IStandardTestProgram
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
                            .AddModelBoundView<ComponentModels.Component5>(name => name.Equals("Component-5", StringComparison.OrdinalIgnoreCase), "Component5")
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
}
