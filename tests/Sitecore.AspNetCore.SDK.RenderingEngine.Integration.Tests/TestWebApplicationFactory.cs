using System.IO;
using GraphQL.Client.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests
{
    public class TestWebApplicationFactory<T>
        : WebApplicationFactory<T>
        where T : class
    {
        public IGraphQLClient MockGraphQLClient { get; set; } = Substitute.For<IGraphQLClient>();

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var options = new WebApplicationOptions
            {
                ContentRootPath = Path.GetFullPath(Directory.GetCurrentDirectory()),
                ApplicationName = typeof(T).Assembly.GetName().Name
            };

            var webBuilder = WebApplication.CreateBuilder(options);
            webBuilder.WebHost.UseTestServer();

            // Ensure MVC can discover controllers in the test assembly (harmless if not used)
            // try
            // {
            //     var mvcBuilder = webBuilder.Services.AddControllers();
            //     mvcBuilder.PartManager.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(typeof(T).Assembly));
            // }
            // catch
            // {
            //     // ignore
            // }

            // Invoke static ConfigureServices(WebApplicationBuilder) if present
            var programType = typeof(T);
            var configureServices = programType.GetMethod("ConfigureServices", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            configureServices?.Invoke(null, new object[] { webBuilder });

            // Replace GraphQL client with mock
            webBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IGraphQLClient), MockGraphQLClient));

            var app = webBuilder.Build();

            // Invoke static ConfigureApp(WebApplication) if present
            var configureApp = programType.GetMethod("ConfigureApp", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            configureApp?.Invoke(null, new object[] { app });

            // Start the app so TestServer.Application is assigned
            app.StartAsync().GetAwaiter().GetResult();
            return app;
        }
    }
}