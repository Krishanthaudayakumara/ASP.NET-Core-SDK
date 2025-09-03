using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProxyKit;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;
using Sitecore.AspNetCore.SDK.Tracking;
using Sitecore.AspNetCore.SDK.Tracking.VisitorIdentification;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Tracking;

/// <summary>
/// Test program for tracking and visitor identification functionality.
/// </summary>
public class TestTrackingProgram : IStandardTestProgram
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.Configure<ForwardedHeadersOptions>(options =>
                    {
                        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    });

                    // Add ProxyKit for visitor identification
                    services.AddProxy();

                    services.AddSitecoreLayoutService()
                        .AddHttpHandler("mock", serviceProvider =>
                        {
                            // This will be configured by TestWebApplicationFactory
                            throw new NotImplementedException("Mock handler should be configured by TestWebApplicationFactory");
                        })
                        .AsDefaultHandler();

                    services.AddSitecoreRenderingEngine(options =>
                        {
                            options.AddDefaultComponentRenderer();
                        })
                        .WithTracking();

                    services.AddSitecoreVisitorIdentification(options =>
                    {
                        // This will be configured by TestWebApplicationFactory
                        options.SitecoreInstanceUri = new Uri("http://localhost");
                    });
                });

                webBuilder.Configure(app =>
                {
                    app.UseForwardedHeaders();

                    // Custom visitor identification middleware for testing - must be before routing
                    app.Use(async (HttpContext context, RequestDelegate next) =>
                    {
                        // Check if this is a visitor identification request
                        if (context.Request.Path.StartsWithSegments("/layouts/System"))
                        {
                            // Get the configured HttpClient factory (which will be our mock)
                            var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                            var httpClient = httpClientFactory.CreateClient();

                            // Create request to the configured Sitecore instance
                            var visitorIdOptions = context.RequestServices.GetRequiredService<IOptions<SitecoreVisitorIdentificationOptions>>();
                            if (visitorIdOptions.Value.SitecoreInstanceUri != null)
                            {
                                // Build the correct path
                                var fullPath = context.Request.Path + context.Request.QueryString;
                                var finalUrl = new Uri(visitorIdOptions.Value.SitecoreInstanceUri, fullPath.ToString());
                                var request = new HttpRequestMessage(HttpMethod.Get, finalUrl);

                                // Apply forwarded headers
                                var ip = context.Connection.RemoteIpAddress;
                                if (ip != null)
                                {
                                    request.Headers.Add("X-Forwarded-For", ip.ToString());
                                }

                                request.Headers.Add("X-Forwarded-Host", context.Request.Host.ToString());
                                request.Headers.Add("X-Forwarded-Proto", context.Request.Scheme);

                                // Copy cookies
                                if (context.Request.Headers.ContainsKey("Cookie"))
                                {
                                    request.Headers.Add("Cookie", context.Request.Headers["Cookie"].ToString());
                                }

                                // Send request and return response
                                var response = await httpClient.SendAsync(request);
                                context.Response.StatusCode = (int)response.StatusCode;

                                if (response.Content != null)
                                {
                                    var content = await response.Content.ReadAsByteArrayAsync();
                                    await context.Response.Body.WriteAsync(content, 0, content.Length);
                                }

                                return; // Don't call next middleware
                            }
                        }

                        // If not a visitor identification request, continue to next middleware
                        await next(context);
                    });

                    app.UseSitecoreRenderingEngine();
                });
            });
}
