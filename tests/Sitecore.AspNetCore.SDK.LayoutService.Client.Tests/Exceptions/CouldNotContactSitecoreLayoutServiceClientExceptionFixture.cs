using Shouldly;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Exceptions;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Exceptions;

public class CouldNotContactSitecoreLayoutServiceClientExceptionFixture
{
    private const string DefaultMessage = "Could not contact the Sitecore layout service.";

    [Theory]
    [AutoNSubstituteData]
    public void CouldNotContactSitecoreLayoutServiceClientException_WithMessage_SetsMessage(string message)
    {
        // Act
        CouldNotContactSitecoreLayoutServiceClientException sut = new(message);

        // Assert
        sut.Message.ShouldBe(message);
    }

    [Theory]
    [AutoNSubstituteData]
    public void CouldNotContactSitecoreLayoutServiceClientException_WithMessageAndException_SetsMessageAndInnerException(
        string message,
        Exception exception)
    {
        // Act
        CouldNotContactSitecoreLayoutServiceClientException sut = new(message, exception);

        // Assert
        sut.Message.ShouldBe(message);
        sut.InnerException.ShouldBe(exception);
    }

    [Fact]
    public void CouldNotContactSitecoreLayoutServiceClientException_WithNoMessage_UsesDefaultMessage()
    {
        // Act
        CouldNotContactSitecoreLayoutServiceClientException sut = new();

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
    }

    [Theory]
    [AutoNSubstituteData]
    public void CouldNotContactSitecoreLayoutServiceClientException_WithException_SetsInnerException(
        Exception exception)
    {
        // Act
        CouldNotContactSitecoreLayoutServiceClientException sut = new(exception);

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
        sut.InnerException.ShouldBe(exception);
    }
}
