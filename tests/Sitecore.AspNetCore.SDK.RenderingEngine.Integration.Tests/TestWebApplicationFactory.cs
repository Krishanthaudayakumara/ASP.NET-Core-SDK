using System.IO;
using System.Reflection;
using GraphQL.Client.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Configuration;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Request.Handlers;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization;
using Sitecore.AspNetCore.SDK.Pages.Configuration;
using Sitecore.AspNetCore.SDK.Pages.Extensions;
using Sitecore.AspNetCore.SDK.Pages.Request.Handlers.GraphQL;
using Sitecore.AspNetCore.SDK.Pages.Services;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Mocks;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;
using Sitecore.AspNetCore.SDK.TestData;
using Sitecore.AspNetCore.SDK.Tracking.VisitorIdentification;
using ProxyKit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests
{
    public class TestWebApplicationFactory<T>
        : WebApplicationFactory<T>
        where T : class
    {
        private bool IsPagesTest => typeof(IPagesTestProgram).IsAssignableFrom(typeof(T));

        public IGraphQLClient MockGraphQLClient { get; set; } = Substitute.For<IGraphQLClient>();

        public MockHttpMessageHandler MockClientHandler { get; set; } = new();

        public Uri LayoutServiceUri { get; set; } = new("http://layout.service");

        protected override IHostBuilder? CreateHostBuilder()
        {
            // Use reflection to call the test program's CreateHostBuilder method
            var createHostBuilderMethod = typeof(T).GetMethod("CreateHostBuilder", BindingFlags.Public | BindingFlags.Static);
            if (createHostBuilderMethod != null)
            {
                return createHostBuilderMethod.Invoke(null, new object[] { Array.Empty<string>() }) as IHostBuilder;
            }

            // Fall back to default behavior if no CreateHostBuilder method found
            return base.CreateHostBuilder();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory())
                   .ConfigureTestServices(services =>
                   {
                       ServiceDescriptor descriptor = new(typeof(IGraphQLClient), MockGraphQLClient);
                       services.Replace(descriptor);

                       // Configure mock layout service handlers
                       services.PostConfigure<SitecoreLayoutClientOptions>(options =>
                       {
                           options.HandlerRegistry.Clear();
                           options.HandlerRegistry["mock"] = serviceProvider =>
                           {
                               HttpClient client = new HttpClient(MockClientHandler) { BaseAddress = LayoutServiceUri };

                               var mockOptions = Substitute.For<IOptionsSnapshot<HttpLayoutRequestHandlerOptions>>();
                               var handlerOptions = new HttpLayoutRequestHandlerOptions();

                               var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

                               // Add default request mapping to generate ?item= query parameter and forward cookies
                               handlerOptions.RequestMap.Add((request, message) =>
                               {
                                   message.RequestUri = message.RequestUri != null
                                       ? request.BuildDefaultSitecoreLayoutRequestUri(message.RequestUri)
                                       : null;

                                   // Forward cookies from the current HttpContext if available
                                   if (httpContextAccessor?.HttpContext?.Request.Headers.ContainsKey("Cookie") == true)
                                   {
                                       var cookieHeaders = httpContextAccessor.HttpContext.Request.Headers["Cookie"];
                                       foreach (var cookieHeader in cookieHeaders)
                                       {
                                           if (!string.IsNullOrEmpty(cookieHeader))
                                           {
                                               message.Headers.Add("Cookie", cookieHeader);
                                           }
                                       }
                                   }
                               });

                               mockOptions.Get(Arg.Any<string>()).Returns(handlerOptions);

                               return new HttpLayoutRequestHandler(
                                   client,
                                   serviceProvider.GetRequiredService<ISitecoreLayoutSerializer>(),
                                   mockOptions,
                                   serviceProvider.GetRequiredService<ILogger<HttpLayoutRequestHandler>>());
                           };

                           if (IsPagesTest)
                           {
                               options.HandlerRegistry["pages"] = serviceProvider =>
                               {
                                   var graphQLClient = serviceProvider.GetRequiredService<IGraphQLClient>();
                                   return new GraphQLEditingServiceHandler(
                                       graphQLClient,
                                       serviceProvider.GetRequiredService<ISitecoreLayoutSerializer>(),
                                       serviceProvider.GetRequiredService<ILogger<GraphQLEditingServiceHandler>>(),
                                       serviceProvider.GetRequiredService<IDictionaryService>());
                               };
                               options.DefaultHandler = "pages";
                           }
                           else
                           {
                               options.DefaultHandler = "mock";
                           }
                       });

                       // Configure visitor identification options for tracking tests
                       bool isTrackingProgram = typeof(T).Name == "TestTrackingProgram";
                       if (isTrackingProgram)
                       {
                           services.PostConfigure<SitecoreVisitorIdentificationOptions>(options =>
                           {
                               options.SitecoreInstanceUri = LayoutServiceUri;
                           });

                           services.Replace(ServiceDescriptor.Singleton<IHttpClientFactory>(_ =>
                           {
                               return new CustomHttpClientFactory(() => new HttpClient(MockClientHandler));
                           }));

                           // Add MVC for TrackingController
                           services.AddRouting()
                                   .AddMvc();
                           services.AddHttpContextAccessor();
                       }

                       if (IsPagesTest)
                       {
                           services.AddRouting()
                                   .AddMvc();

                           services.AddSitecoreLayoutService()
                                   .AddSitecorePagesHandler();

                           services.AddSitecoreRenderingEngine(options =>
                                   {
                                       options.AddDefaultPartialView("_ComponentNotFound");
                                   })
                                   .WithSitecorePages(TestConstants.ContextId, options => { options.EditingSecret = TestConstants.JssEditingSecret; });

                           // Configure PagesOptions for the middleware
                           services.Configure<PagesOptions>(options =>
                           {
                               options.ConfigEndpoint = TestConstants.ConfigRoute;
                           });
                       }
                       else
                       {
                           // For standard tests, let the test program handle its own configuration
                           // Only add essential services that might be needed
                           // Don't override the rendering engine configuration
                       }
                   })
                   .Configure(app =>
                   {
                       if (IsPagesTest)
                       {
                           app.UseMiddleware<Sitecore.AspNetCore.SDK.Pages.Middleware.PagesRenderMiddleware>();
                           app.UseRouting();
                           app.UseEndpoints(endpoints =>
                           {
                               // Map the config endpoint to PagesSetup controller
                               endpoints.MapControllerRoute(
                                   name: "pagesconfig",
                                   pattern: "api/editing/config",
                                   defaults: new { controller = "PagesSetup", action = "Config" });

                               // Map the render endpoint to PagesSetup controller
                               endpoints.MapControllerRoute(
                                   name: "pagesrender",
                                   pattern: "api/editing/render",
                                   defaults: new { controller = "PagesSetup", action = "Render" });

                               // Map the default route to Pages controller
                               endpoints.MapControllerRoute(
                                   name: "default",
                                   pattern: "{controller=Pages}/{action=Index}");
                           });
                       }
                       else
                       {
                           // For tracking programs, add visitor identification middleware before routing
                           bool isTrackingProgram = typeof(T).Name == "TestTrackingProgram";
                           if (isTrackingProgram)
                           {
                               app.UseForwardedHeaders();

                               // Custom visitor identification middleware for testing
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
                                           var finalUrl = new Uri(visitorIdOptions.Value.SitecoreInstanceUri, fullPath);
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
                           }

                           app.UseRouting();

                           // TestBasicProgram and TestTrackingProgram handle their own global middleware configuration
                           // Only add global rendering engine middleware for programs that don't configure it themselves
                           bool isBasicProgram = typeof(T).Name == "TestBasicProgram";
                           if (!isBasicProgram && !isTrackingProgram)
                           {
                               app.UseSitecoreRenderingEngine();
                           }

                           app.UseEndpoints(configure =>
                           {
                               configure.MapDefaultControllerRoute();
                           });
                       }
                   });
        }
    }
}