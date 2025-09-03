using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;
using Sitecore.AspNetCore.SDK.Tracking;
using Sitecore.AspNetCore.SDK.Tracking.VisitorIdentification;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Tracking;

/// <summary>
/// Simple test program for tracking functionality without proxy requirements.
/// </summary>
public class TestSimpleTrackingProgram : IStandardTestProgram
{
    /// <summary>
    /// Creates the host builder for the test application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>The configured host builder.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.Configure<ForwardedHeadersOptions>(options =>
                    {
                        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
                    });

                    services.AddRouting()
                            .AddMvc();

                    services.AddSitecoreLayoutService()
                        .AddHttpHandler("mock", _ =>
                        {
                            // This will be configured by TestWebApplicationFactory
                            throw new NotImplementedException("Mock handler should be configured by TestWebApplicationFactory");
                        })
                        .AsDefaultHandler();

                    services.AddSitecoreRenderingEngine(options =>
                        {
                            options.AddDefaultPartialView("_ComponentNotFound");
                            options.AddDefaultComponentRenderer();
                        })
                        .WithTracking();

                    services.AddSitecoreVisitorIdentification(options =>
                    {
                        // This will be configured by TestWebApplicationFactory
                        options.SitecoreInstanceUri = new Uri("http://localhost");
                    });

                    // Add HttpContextAccessor for cookie forwarding
                    services.AddHttpContextAccessor();
                });

                webBuilder.Configure(app =>
                {
                    app.UseForwardedHeaders();
                    app.UseRouting();
                    app.UseSitecoreVisitorIdentification();
                    app.UseSitecoreRenderingEngine();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapDefaultControllerRoute();
                    });
                });
            });
}
