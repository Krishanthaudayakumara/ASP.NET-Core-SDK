using Shouldly;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Exceptions;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Exceptions;

public class InvalidRequestSitecoreLayoutServiceClientExceptionFixture
{
    private const string DefaultMessage = "An invalid request was sent to the Sitecore layout service.";

    [Theory]
    [AutoNSubstituteData]
    public void InvalidRequestSitecoreLayoutServiceClientException_WithMessage_SetsMessage(string message)
    {
        // Act
        InvalidRequestSitecoreLayoutServiceClientException sut = new(message);

        // Assert
        sut.Message.ShouldBe(message);
    }

    [Theory]
    [AutoNSubstituteData]
    public void InvalidRequestSitecoreLayoutServiceClientException_WithMessageAndException_SetsMessageAndInnerException(
        string message,
        Exception exception)
    {
        // Act
        InvalidRequestSitecoreLayoutServiceClientException sut = new(message, exception);

        // Assert
        sut.Message.ShouldBe(message);
        sut.InnerException.ShouldBe(exception);
    }

    [Fact]
    public void InvalidRequestSitecoreLayoutServiceClientException_WithNoMessage_UsesDefaultMessage()
    {
        // Act
        InvalidRequestSitecoreLayoutServiceClientException sut = new();

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
    }

    [Theory]
    [AutoNSubstituteData]
    public void InvalidRequestSitecoreLayoutServiceClientException_WithException_SetsInnerException(
        Exception exception)
    {
        // Act
        InvalidRequestSitecoreLayoutServiceClientException sut = new(exception);

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
        sut.InnerException.ShouldBe(exception);
    }
}
