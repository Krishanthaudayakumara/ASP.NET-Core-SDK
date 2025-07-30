using AutoFixture;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Configuration;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Interfaces;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Request;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Request.Handlers;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Request.Handlers.GraphQL;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Extensions;

public class SitecoreLayoutClientBuilderExtensionsFixture
{
    // ReSharper disable once UnusedMember.Global - Used by testing framework
    public static Action<IFixture> AutoSetup => f =>
    {
        SitecoreLayoutClientBuilder builder = new(new ServiceCollection());
        f.Inject(builder);
    };

    [Fact]
    public void AddHandler_NullBuilder_Throws()
    {
        // Arrange
        Action action = () => SitecoreLayoutClientBuilderExtensions.AddHandler<ILayoutRequestHandler>(null!, "string");

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("builder");
    }

    [Theory]
    [MemberAutoNSubstituteData(nameof(EmptyStrings))]
    public void AddHandler_InvalidName_Throws(string value, SitecoreLayoutClientBuilder builder)
    {
        // Arrange
        Action action = () => builder.AddHandler<ILayoutRequestHandler>(value);

        // Act / Assert
        var ex = Should.Throw<ArgumentException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("name");
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddHandler_Throws_IfTServiceIsAnInterface(SitecoreLayoutClientBuilder builder, string handlerName)
    {
        // Arrange
        Action action = () => builder.AddHandler<ILayoutRequestHandler>(handlerName);

        // Act / Assert
        var ex = Should.Throw<ArgumentException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Assert exception.Message manually} as layout services.");
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddHandler_Throws_IfTServiceIsAnAbstractClass(SitecoreLayoutClientBuilder builder, string handlerName)
    {
        // Arrange
        Action action = () => builder.AddHandler<TestAbstractSitecoreLayoutRequestHandler>(handlerName);

        // Act / Assert
        var ex = Should.Throw<ArgumentException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Assert exception.Message manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddHandler_CreatesFactory_IfFactoryIsNull(SitecoreLayoutClientBuilder builder, string handlerName)
    {
        // Arrange / Act
        ILayoutRequestHandlerBuilder<HttpLayoutRequestHandler> result = builder.AddHandler<HttpLayoutRequestHandler>(handlerName);

        // Assert
        ServiceProvider provider = result.Services.BuildServiceProvider();
        SitecoreLayoutClientOptions options = provider.GetRequiredService<IOptions<SitecoreLayoutClientOptions>>().Value;
        options.HandlerRegistry[handlerName].ShouldNotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddHandler_UsesTheProvidedFactory_IfFactoryIsNotNull(
        SitecoreLayoutClientBuilder builder,
        string handlerName,
        HttpLayoutRequestHandler service)
    {
        // Arrange / Act
        ILayoutRequestHandlerBuilder<HttpLayoutRequestHandler> result = builder.AddHandler(handlerName, _ => service);

        // Assert
        ServiceProvider provider = result.Services.BuildServiceProvider();
        SitecoreLayoutClientOptions options = provider.GetRequiredService<IOptions<SitecoreLayoutClientOptions>>().Value;
        ILayoutRequestHandler instance = options.HandlerRegistry[handlerName].Invoke(provider);
        instance.ShouldBeSameAs(service);
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddHandler_WithValidValues_ReturnsNewBuilderWithCorrectValues(
        SitecoreLayoutClientBuilder builder,
        string handlerName)
    {
        // Arrange / Act
        ILayoutRequestHandlerBuilder<HttpLayoutRequestHandler> result = builder.AddHandler<HttpLayoutRequestHandler>(handlerName);

        // Assert
        result.Should().BeOfType<SitecoreLayoutRequestHandlerBuilder<HttpLayoutRequestHandler>>();
        result.Services.ShouldBeSameAs(builder.Services);
        result.HandlerName.ShouldBe(handlerName);
    }

    [Fact]
    public void WithDefaultRequestOptions_BuilderIsNUll_ThrowsArgumentNullException()
    {
        // Arrange
        Func<ISitecoreLayoutClientBuilder> act =
            () => SitecoreLayoutClientBuilderExtensions.WithDefaultRequestOptions(null!, null!);

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually// TODO: Assert exception.Message manually");
    }

    [Theory]
    [AutoNSubstituteData]
    public void WithDefaultRequestOptions_ConfigureActionIsNUll_ThrowsArgumentNullException(SitecoreLayoutClientBuilder builder)
    {
        // Arrange
        Func<ISitecoreLayoutClientBuilder> act =
            () => builder.WithDefaultRequestOptions(null!);

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually// TODO: Assert exception.Message manually");
    }

    [Theory]
    [AutoNSubstituteData]
    public void WithDefaultRequestOptions_RequestDefaultsIsNotNull_ServiceProviderReturnsOptionsWithRequestDefaults(SitecoreLayoutClientBuilder builder)
    {
        // Act
        ISitecoreLayoutClientBuilder result = builder.WithDefaultRequestOptions(_ => { });

        // Assert
        ServiceProvider provider = result.Services.BuildServiceProvider();
        SitecoreLayoutRequestOptions options = provider.GetRequiredService<IOptions<SitecoreLayoutRequestOptions>>().Value;
        options.RequestDefaults.ShouldNotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void WithDefaultRequestOptions_RequestDefaultsHasApiKeySpecified_ServiceProviderReturnsOptionsWithRequestDefaultsContainingApiKey(SitecoreLayoutClientBuilder builder)
    {
        // Act
        ISitecoreLayoutClientBuilder result = builder.WithDefaultRequestOptions(o => { o.ApiKey("test_api_key"); });

        // Assert
        ServiceProvider provider = result.Services.BuildServiceProvider();
        SitecoreLayoutRequestOptions options = provider.GetRequiredService<IOptions<SitecoreLayoutRequestOptions>>().Value;
        options.RequestDefaults.ShouldNotBeNull();
        options.RequestDefaults.ShouldBeOfType<SitecoreLayoutRequest>();
        options.RequestDefaults.Should().ContainKey(RequestKeys.ApiKey);
        options.RequestDefaults.ApiKey().ShouldBe("test_api_key");
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddGraphQLHandler_Minimal_IsValid(SitecoreLayoutClientBuilder builder, string name, string siteName, string apiKey, Uri uri)
    {
        // Act
        ILayoutRequestHandlerBuilder<GraphQLLayoutServiceHandler> result = builder.AddGraphQLHandler(name, siteName, apiKey, uri);

        // Assert
        ServiceProvider provider = result.Services.BuildServiceProvider();
        SitecoreLayoutRequestOptions options = provider.GetRequiredService<IOptions<SitecoreLayoutRequestOptions>>().Value;
        options.RequestDefaults.ApiKey().ShouldBe(apiKey);
        options.RequestDefaults.SiteName().ShouldBe(siteName);
        options.RequestDefaults.Language().ShouldBe("en");
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddGraphQLWithContextHandler_Minimal_IsValid(SitecoreLayoutClientBuilder builder, string contextId)
    {
        // Act
        ILayoutRequestHandlerBuilder<GraphQLLayoutServiceHandler> result = builder.AddGraphQLWithContextHandler("Test", contextId);

        // Assert
        ServiceProvider provider = result.Services.BuildServiceProvider();
        SitecoreLayoutRequestOptions options = provider.GetRequiredService<IOptions<SitecoreLayoutRequestOptions>>().Value;
        options.RequestDefaults.ContextId().ShouldBe(contextId);
        options.RequestDefaults.SiteName().ShouldBeNull();
        options.RequestDefaults.Language().ShouldBe("en");
    }

    private static IEnumerable<object[]> EmptyStrings()
    {
        yield return [null!];
        yield return [string.Empty];
        yield return ["\t\t   "];
    }
}
