using GraphQL.Client.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Benchmarks;

public class TestWebApplicationFactory<T> : WebApplicationFactory<T>
    where T : class
    {
        private readonly List<Action<IServiceCollection>> _serviceConfigurations = new();
        private readonly List<Action<IApplicationBuilder>> _appConfigurations = new();
        private readonly List<Action<WebHostBuilderContext, IConfigurationBuilder>> _configurationActions = new();
        private readonly Dictionary<string, string> _settings = new();

        public IGraphQLClient MockGraphQLClient { get; set; } = Substitute.For<IGraphQLClient>();

        public TestWebApplicationFactory<T> ConfigureServices(Action<IServiceCollection> configureServices)
        {
            _serviceConfigurations.Add(configureServices);
            return this;
        }

        public TestWebApplicationFactory<T> Configure(Action<IApplicationBuilder> configureApp)
        {
            _appConfigurations.Add(configureApp);
            return this;
        }

        public TestWebApplicationFactory<T> ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _configurationActions.Add(configureDelegate);
            return this;
        }

        public TestWebApplicationFactory<T> UseSetting(string key, string value)
        {
            _settings[key] = value;
            return this;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(Path.GetFullPath(Directory.GetCurrentDirectory()));

            // Apply settings
            foreach (var setting in _settings)
            {
                builder.UseSetting(setting.Key, setting.Value);
            }

            // Apply configuration actions
            foreach (var configAction in _configurationActions)
            {
                builder.ConfigureAppConfiguration(configAction);
            }

            builder.ConfigureTestServices(services =>
            {
                // Apply service configurations
                foreach (var serviceConfig in _serviceConfigurations)
                {
                    serviceConfig(services);
                }

                // Replace GraphQL client with mock
                ServiceDescriptor descriptor = new(typeof(IGraphQLClient), MockGraphQLClient);
                services.Replace(descriptor);
            });

            // Configure app pipeline
            if (_appConfigurations.Count > 0)
            {
                builder.Configure(app =>
                {
                    foreach (var appConfig in _appConfigurations)
                    {
                        appConfig(app);
                    }
                });
            }
        }
    }