using System;
using System.Net.Http;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding
{
    public class TestBindingProgram
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddRouting()
                            .AddMvc();

            builder.Services.AddSitecoreLayoutService()
                            .AddHttpHandler("mock", _ => new HttpClient { BaseAddress = new Uri("http://layout.service") })
                            .AsDefaultHandler();

            builder.Services.AddSitecoreRenderingEngine();
        }

        public static void ConfigureApp(WebApplication app)
        {
            app.UseSitecoreRenderingEngine();
            app.UseRouting();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Pages}/{action=Index}");
        }

    // Main intentionally omitted so this file can be consumed by WebApplicationFactory in tests.
    }
}
