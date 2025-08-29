using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests;

/// <summary>
/// Program class for basic integration testing without Pages support.
/// </summary>
public class TestProgram
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRouting()
                        .AddMvc();

        builder.Services.AddSitecoreLayoutService();
        builder.Services.AddSitecoreRenderingEngine();

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
