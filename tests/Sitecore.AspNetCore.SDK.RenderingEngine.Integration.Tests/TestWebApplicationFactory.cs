using System.IO;
using GraphQL.Client.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Sitecore.AspNetCore.SDK.AutoFixture.Mocks;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Configuration;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Request.Handlers;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization;
using Sitecore.AspNetCore.SDK.Pages.Request.Handlers.GraphQL;
using Sitecore.AspNetCore.SDK.Pages.Services;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Interfaces;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests
{
    public class TestWebApplicationFactory<T>
        : WebApplicationFactory<T>
        where T : class
    {
        public IGraphQLClient MockGraphQLClient { get; set; } = Substitute.For<IGraphQLClient>();

        public MockHttpMessageHandler MockClientHandler { get; set; } = new();

        public Uri LayoutServiceUri { get; set; } = new("http://layout.service");

        private bool IsPagesTest => typeof(IPagesTestProgram).IsAssignableFrom(typeof(T));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory())
                   .ConfigureTestServices(services =>
                   {
                       // Replace GraphQL client with mock for testing
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
                   });
        }
    }
}