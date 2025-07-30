using Shouldly;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Exceptions;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Exceptions;

public class SitecoreLayoutServiceMessageConfigurationExceptionFixture
{
    private const string DefaultMessage = "An error occurred while configuring the HTTP message.";

    [Theory]
    [AutoNSubstituteData]
    public void SitecoreLayoutServiceMessageConfigurationException_WithMessage_SetsMessage(string message)
    {
        // Act
        SitecoreLayoutServiceMessageConfigurationException sut = new(message);

        // Assert
        sut.Message.ShouldBe(message);
    }

    [Theory]
    [AutoNSubstituteData]
    public void SitecoreLayoutServiceMessageConfigurationException_WithMessageAndException_SetsMessageAndInnerException(
        string message,
        Exception exception)
    {
        // Act
        SitecoreLayoutServiceMessageConfigurationException sut = new(message, exception);

        // Assert
        sut.Message.ShouldBe(message);
        sut.InnerException.ShouldBe(exception);
    }

    [Fact]
    public void SitecoreLayoutServiceMessageConfigurationException_WithNoMessage_UsesDefaultMessage()
    {
        // Act
        SitecoreLayoutServiceMessageConfigurationException sut = new();

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
    }

    [Theory]
    [AutoNSubstituteData]
    public void SitecoreLayoutServiceMessageConfigurationException_WithException_SetsInnerException(
        Exception exception)
    {
        // Act
        SitecoreLayoutServiceMessageConfigurationException sut = new(exception);

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
        sut.InnerException.ShouldBe(exception);
    }
}
