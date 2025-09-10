using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests
{
    public class TestWebApplicationFactoryWithServices<TEntryPoint> : WebApplicationFactory<TEntryPoint>
        where TEntryPoint : class
    {
        public Action<IServiceCollection>? ConfigureTestServicesAction { get; set; }

        public Action<IApplicationBuilder>? ConfigureTestAppAction { get; set; }

        public Uri BaseAddressOverride { get; set; } = new("http://localhost");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseContentRoot(Path.GetFullPath(Directory.GetCurrentDirectory()))
                .ConfigureTestServices(services =>
                {
                    // minimal defaults similar to previous TestServerBuilder.PrepareDefault()
                    services.AddRouting();
                    ConfigureTestServicesAction?.Invoke(services);

                    // If a test app action was provided, register an IStartupFilter which will
                    // append the action to the application's pipeline without replacing the
                    // application's existing Configure delegate from the TestProgram.
                    if (ConfigureTestAppAction is not null)
                    {
                        services.AddSingleton<IStartupFilter>(new DelegateStartupFilter(ConfigureTestAppAction));
                    }
                });
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            TestServer server = base.CreateServer(builder);
            server.BaseAddress = BaseAddressOverride;
            return server;
        }

        private class DelegateStartupFilter : IStartupFilter
        {
            private readonly Action<IApplicationBuilder> _configure;

            public DelegateStartupFilter(Action<IApplicationBuilder> configure)
            {
                _configure = configure;
            }

            // Ensure we append our middleware after the application's Configure has run.
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    // Invoke test-supplied app configuration, then continue to app's Configure.
                    _configure(app);
                    next(app);
                };
            }
        }
    }
}
