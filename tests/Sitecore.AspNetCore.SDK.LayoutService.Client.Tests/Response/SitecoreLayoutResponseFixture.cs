using AutoFixture.Idioms;
using Shouldly;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.AutoFixture.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Response;

public class SitecoreLayoutResponseFixture
{
    [Theory]
    [AutoNSubstituteData]
    public void Ctor_IsGuarded(GuardClauseAssertion guard)
    {
        // Act / Assert
        guard.VerifyConstructors<SitecoreLayoutResponse>();
    }

    [Fact]
    public void Ctor_WithErrors_SetsDefaults()
    {
        // Arrange / Act
        SitecoreLayoutResponse sut = new([], []);

        // Assert
        sut.Metadata.ShouldBeNull();
        sut.Content.ShouldBe(default);
        sut.HasErrors.ShouldBeFalse();
        sut.Errors.ShouldNotBeNull();
        sut.Errors.ShouldBeEmpty();
        sut.Request.ShouldBeEmpty();
    }

    [Fact]
    public void Ctor_NoErrors_SetsDefaults()
    {
        // Arrange / Act
        SitecoreLayoutResponse sut = new([]);

        // Assert
        sut.Metadata.ShouldBeNull();
        sut.Content.ShouldBe(default);
        sut.HasErrors.ShouldBeFalse();
        sut.Errors.ShouldBeEmpty();
        sut.Request.ShouldBeEmpty();
    }
}
