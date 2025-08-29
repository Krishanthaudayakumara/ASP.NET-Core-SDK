using System.IO;
using GraphQL.Client.Abstractions;
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
using Sitecore.AspNetCore.SDK.TestData;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests
{
    public class TestWebApplicationFactory<T>
        : WebApplicationFactory<T>
        where T : class
    {
        public IGraphQLClient MockGraphQLClient { get; set; } = Substitute.For<IGraphQLClient>();

        public MockHttpMessageHandler MockClientHandler { get; set; } = new();

        public Uri LayoutServiceUri { get; set; } = new("http://layout.service");

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
                           // Clear all existing handlers and set up our mock as the default
                           options.HandlerRegistry.Clear();
                           options.HandlerRegistry["mock"] = serviceProvider =>
                           {
                               HttpClient client = new HttpClient(MockClientHandler) { BaseAddress = LayoutServiceUri };

                               // Create mock options since IOptionsSnapshot is scoped
                               var mockOptions = Substitute.For<IOptionsSnapshot<HttpLayoutRequestHandlerOptions>>();
                               var handlerOptions = new HttpLayoutRequestHandlerOptions();
                               mockOptions.Get(Arg.Any<string>()).Returns(handlerOptions);

                               return new HttpLayoutRequestHandler(
                                   client,
                                   serviceProvider.GetRequiredService<ISitecoreLayoutSerializer>(),
                                   mockOptions,
                                   serviceProvider.GetRequiredService<ILogger<HttpLayoutRequestHandler>>());
                           };

                           // For Pages tests, also add a "pages" handler
                           if (typeof(T).Name.Contains("Pages"))
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

                       // Check if we're configuring for Pages tests
                       if (typeof(T).Name.Contains("Pages"))
                       {
                           // Pages-specific configuration
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
                           // Standard configuration for other tests
                           services.AddRouting()
                                   .AddMvc();

                           services.AddSitecoreLayoutService()
                                   .AddHttpHandler("mock", _ => new HttpClient() { BaseAddress = new Uri("http://layout.service") })
                                   .AsDefaultHandler();

                           services.AddSitecoreRenderingEngine();
                       }
                   })
                   .Configure(app =>
                   {
                       // Check if we're configuring for Pages tests
                       if (typeof(T).Name.Contains("Pages"))
                       {
                           // Pages-specific middleware pipeline
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
                           // Standard middleware pipeline
                           app.UseRouting();
                           app.UseSitecoreRenderingEngine();
                           app.UseEndpoints(configure =>
                           {
                               configure.MapDefaultControllerRoute();
                           });
                       }
                   });
        }
    }
}