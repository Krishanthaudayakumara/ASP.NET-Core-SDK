using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Binding.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

/// <summary>
/// Test program class for model binding scenarios.
/// </summary>
public class TestModelBindingProgram : IBindingTestProgram
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseContentRoot(Directory.GetCurrentDirectory())
                          .ConfigureServices(services =>
                          {
                              services.AddRouting()
                                      .AddMvc(options => options.AddSitecoreModelBinderProviders());

                              services.AddSitecoreLayoutService()
                                      .AddHttpHandler("mock", _ => new HttpClient() { BaseAddress = new Uri("http://layout.service") })
                                      .AsDefaultHandler();

                              services.AddSitecoreRenderingEngine();
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
