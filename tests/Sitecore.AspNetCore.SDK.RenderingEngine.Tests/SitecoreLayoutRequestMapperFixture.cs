using AutoFixture;
using AutoFixture.Idioms;
using Shouldly;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.AutoFixture.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Request;
using Sitecore.AspNetCore.SDK.RenderingEngine.Configuration;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Mappers;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Tests;

public class SitecoreLayoutRequestMapperFixture
{
    // ReSharper disable once UnusedMember.Global - Used by testing framework
    public static Action<IFixture> AutoSetup => f =>
    {
        f.Register<string, PathString>(s => new PathString("/" + s));
        IOptions<RenderingEngineOptions>? optionSub = f.Freeze<IOptions<RenderingEngineOptions>>();
        RenderingEngineOptions options = new();
        optionSub.Value.Returns(options);
    };

    [Theory]
    [AutoNSubstituteData]
    public void Ctor_InvalidArgs_Throws(GuardClauseAssertion guard)
    {
        guard.VerifyConstructors<SitecoreLayoutRequestMapper>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Ctor_NullRequestMappings_Throws(IOptions<RenderingEngineOptions> options)
    {
        // Arrange
        options.Value.RequestMappings = null!;
        Action action = () => _ = new SitecoreLayoutRequestMapper(options);

        // Act / Assert
        var ex = Should.Throw<ArgumentException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Assert exception.Message manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public void Map_WithNullHttpRequest_ThrowsException(SitecoreLayoutRequestMapper sut)
    {
        // Arrange
        Func<SitecoreLayoutRequest> act =
            () => sut.Map(null!);

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually// TODO: Assert exception.Message manually");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Map_NullRequest_ThrowsException(SitecoreLayoutRequestMapper sut)
    {
        // Arrange
        Action action = () => sut.Map(null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("request");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Map_WithRequest_ReturnsMappedRequest(
        IOptions<RenderingEngineOptions> options,
        HttpRequest request)
    {
        // Arrange
        options.Value.MapToRequest((_, sc) => sc.Path("SET"));
        SitecoreLayoutRequestMapper sut = new(options);

        // Act
        SitecoreLayoutRequest result = sut.Map(request);

        // Assert
        result.Path().ShouldBe("SET");
    }
}
