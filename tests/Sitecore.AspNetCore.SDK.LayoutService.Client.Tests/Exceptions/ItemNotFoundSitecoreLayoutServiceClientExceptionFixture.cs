using Shouldly;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Exceptions;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Exceptions;

public class ItemNotFoundSitecoreLayoutServiceClientExceptionFixture
{
    private const string DefaultMessage = "The Sitecore layout service returned an item not found response.";

    [Theory]
    [AutoNSubstituteData]
    public void ItemNotFoundSitecoreLayoutServiceClientException_WithMessage_SetsMessage(string message)
    {
        // Act
        ItemNotFoundSitecoreLayoutServiceClientException sut = new(message);

        // Assert
        sut.Message.ShouldBe(message);
    }

    [Theory]
    [AutoNSubstituteData]
    public void ItemNotFoundSitecoreLayoutServiceClientException_WithMessageAndException_SetsMessageAndInnerException(
        string message,
        Exception exception)
    {
        // Act
        ItemNotFoundSitecoreLayoutServiceClientException sut = new(message, exception);

        // Assert
        sut.Message.ShouldBe(message);
        sut.InnerException.ShouldBe(exception);
    }

    [Fact]
    public void ItemNotFoundSitecoreLayoutServiceClientException_WithNoMessage_UsesDefaultMessage()
    {
        // Act
        ItemNotFoundSitecoreLayoutServiceClientException sut = new();

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
    }

    [Theory]
    [AutoNSubstituteData]
    public void ItemNotFoundSitecoreLayoutServiceClientException_WithException_SetsInnerException(
        Exception exception)
    {
        // Act
        ItemNotFoundSitecoreLayoutServiceClientException sut = new(exception);

        // Assert
        sut.Message.ShouldBe(DefaultMessage);
        sut.InnerException.ShouldBe(exception);
    }
}
