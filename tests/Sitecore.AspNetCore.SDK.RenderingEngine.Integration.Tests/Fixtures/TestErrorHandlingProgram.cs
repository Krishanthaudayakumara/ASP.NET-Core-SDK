using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures;

public class TestErrorHandlingProgram : IStandardTestProgram
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
                        options.AddDefaultPartialView("_ComponentNotFound");
                        options.AddDefaultComponentRenderer();
                    });
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseSitecoreRenderingEngine();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllerRoute(
                            name: "default",
                            pattern: "{controller=Home}/{action=Index}/{id?}");
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

        builder.Services.AddSitecoreRenderingEngine();

        WebApplication app = builder.Build();

        app.UseRouting();
        app.UseSitecoreRenderingEngine();

        // Map controllers for error handling tests
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
