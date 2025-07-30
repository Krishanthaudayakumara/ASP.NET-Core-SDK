using Shouldly;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Exceptions;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Exceptions;

public class SitecoreLayoutServiceClientExceptionFixture
{
    private const string DefaultMessage = "An error occurred with the Sitecore layout service.";

    [Theory]
    [AutoNSubstituteData]
    public void SitecoreLayoutServiceClientException_WithMessage_SetsMessage(string message)
    {
        // Act
        SitecoreLayoutServiceClientException sut = new(message);

        // Assert
        sut.Message.ShouldBe(message);
    }

    [Theory]
    [AutoNSubstituteData]
    public void SitecoreLayoutServiceClientException_WithMessageAndException_SetsMessageAndInnerException(
        string message,
        Exception exception)
    {
        // Act
        SitecoreLayoutServiceClientException sut = new(message, exception);

        // Assert
        sut.Message.ShouldBe(message);
        sut.InnerException.ShouldBe(exception);
    }

    [Fact]
    public void SitecoreLayoutServiceClientException_WithNoMessage_UsesDefaultMessage()
    {
        // Act
        SitecoreLayoutServiceClientException sut = new();

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
    }

    [Theory]
    [AutoNSubstituteData]
    public void SitecoreLayoutServiceClientException_WithException_SetsInnerException(
        Exception exception)
    {
        // Act
        SitecoreLayoutServiceClientException sut = new(exception);

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
        sut.InnerException.ShouldBe(exception);
    }
}
