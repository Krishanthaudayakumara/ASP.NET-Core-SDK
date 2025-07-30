using Shouldly;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Exceptions;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Exceptions;

public class InvalidResponseSitecoreLayoutServiceClientExceptionFixture
{
    private const string DefaultMessage = "The Sitecore layout service returned a response in an invalid format.";

    [Theory]
    [AutoNSubstituteData]
    public void InvalidResponseSitecoreLayoutServiceClientException_WithMessage_SetsMessage(string message)
    {
        // Act
        InvalidResponseSitecoreLayoutServiceClientException sut = new(message);

        // Assert
        sut.Message.ShouldBe(message);
    }

    [Theory]
    [AutoNSubstituteData]
    public void InvalidResponseSitecoreLayoutServiceClientException_WithMessageAndException_SetsMessageAndInnerException(
        string message,
        Exception exception)
    {
        // Act
        InvalidResponseSitecoreLayoutServiceClientException sut = new(message, exception);

        // Assert
        sut.Message.ShouldBe(message);
        sut.InnerException.ShouldBe(exception);
    }

    [Fact]
    public void InvalidResponseSitecoreLayoutServiceClientException_WithNoMessage_UsesDefaultMessage()
    {
        // Act
        InvalidResponseSitecoreLayoutServiceClientException sut = new();

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
    }

    [Theory]
    [AutoNSubstituteData]
    public void InvalidResponseSitecoreLayoutServiceClientException_WithException_SetsInnerException(
        Exception exception)
    {
        // Act
        InvalidResponseSitecoreLayoutServiceClientException sut = new(exception);

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
        sut.InnerException.ShouldBe(exception);
    }
}
