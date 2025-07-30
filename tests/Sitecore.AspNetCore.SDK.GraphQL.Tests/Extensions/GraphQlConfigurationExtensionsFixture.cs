using AutoFixture.Xunit2;
using Shouldly;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.GraphQL.Client.Models;
using Sitecore.AspNetCore.SDK.GraphQL.Exceptions;
using Sitecore.AspNetCore.SDK.GraphQL.Extensions;
using Sitecore.AspNetCore.SDK.GraphQL.Properties;
using Xunit;

namespace Sitecore.AspNetCore.SDK.GraphQL.Tests.Extensions;

public class GraphQLConfigurationExtensionsFixture
{
    [Theory]
    [AutoNSubstituteData]
    public void AddGraphQLClient_NullProperties_ThrowsExceptions(IServiceCollection serviceCollection)
    {
        Func<IServiceCollection> servicesNull =
            () => GraphQLConfigurationExtensions.AddGraphQLClient(null!, null!);
        Func<IServiceCollection> configNull =
            () => serviceCollection.AddGraphQLClient(null!);

        var ex = Should.Throw<ArgumentNullException>(() => servicesNull()); // TODO: Assert exception properties manually// TODO: Assert exception.ParamName manually;
        var ex = Should.Throw<ArgumentNullException>(() => configNull()); // TODO: Assert exception properties manually// TODO: Assert exception.ParamName manually;
    }

    [Fact]
    public void AddGraphQLClient_EmptyApiKey_InConfiguration_ThrowsExceptions()
    {
        Func<IServiceCollection> act =
            () => Substitute.For<IServiceCollection>().AddGraphQLClient(_ => { });
        var ex = Should.Throw<InvalidGraphQLConfigurationException>(() => act()); // TODO: Assert exception properties manually
            // TODO: Assert exception.Message manually;
    }

    [Theory]
    [AutoData]
    public void AddGraphQLClient_EmptyEndpoint_WithApiKey_ThrowsExceptions(string apiKey)
    {
        Func<IServiceCollection> act =
            () => Substitute.For<IServiceCollection>().AddGraphQLClient(options =>
            {
                options.ApiKey = apiKey;
            });
        var ex = Should.Throw<InvalidGraphQLConfigurationException>(() => act()); // TODO: Assert exception properties manually
            // TODO: Assert exception.Message manually;
    }

    [Theory]
    [AutoData]
    public void AddGraphQLClient_EmptyEndpointUri_WithContextId_UsesDefault(string contextId)
    {
        // Arrange
        ServiceCollection serviceCollection = [];

        // Act
        serviceCollection.AddGraphQLClient(configuration =>
        {
            configuration.ContextId = contextId;
        });
        GraphQLHttpClient? graphQlClient = serviceCollection.BuildServiceProvider().GetService<IGraphQLClient>() as GraphQLHttpClient;

        // Assert
        graphQlClient!.Options.EndPoint!.OriginalString.ShouldContain(SitecoreGraphQLClientOptions.DefaultEdgeEndpoint.OriginalString);
    }

    [Theory]
    [AutoData]
    public void AddGraphQLClient_AddConfiguredGraphQLClient_To_ServiceCollection(string apiKey, Uri endpointUri, string defaultSiteName)
    {
        // Arrange
        ServiceCollection serviceCollection = [];

        // Act
        serviceCollection.AddGraphQLClient(
            configuration =>
            {
                configuration.ApiKey = apiKey;
                configuration.EndPoint = endpointUri;
                configuration.DefaultSiteName = defaultSiteName;
            });
        GraphQLHttpClient? graphQlClient = serviceCollection.BuildServiceProvider().GetService<IGraphQLClient>() as GraphQLHttpClient;

        // Assert
        graphQlClient.ShouldNotBeNull();
        graphQlClient!.Options.EndPoint.ShouldBe(endpointUri);
        graphQlClient.HttpClient.DefaultRequestHeaders.Contains("sc_apikey").ShouldBeTrue();
        apiKey.ShouldBe(graphQlClient.HttpClient.DefaultRequestHeaders.GetValues("sc_apikey").FirstOrDefault());
    }

    [Theory]
    [AutoData]
    public void AddGraphQLClient_WithContext_To_ServiceCollection(string contextId, Uri endpointUri, string defaultSiteName)
    {
        // Arrange
        ServiceCollection serviceCollection = [];

        // Act
        serviceCollection.AddGraphQLClient(
            configuration =>
            {
                configuration.ContextId = contextId;
                configuration.EndPoint = endpointUri;
                configuration.DefaultSiteName = defaultSiteName;
            });
        GraphQLHttpClient? graphQlClient = serviceCollection.BuildServiceProvider().GetService<IGraphQLClient>() as GraphQLHttpClient;

        // Assert
        graphQlClient.ShouldNotBeNull();
        graphQlClient!.Options.EndPoint!.Host.ShouldBe(endpointUri.Host);
        graphQlClient.Options.EndPoint.Query.ShouldContain($"sitecoreContextId={contextId}");
    }
}
