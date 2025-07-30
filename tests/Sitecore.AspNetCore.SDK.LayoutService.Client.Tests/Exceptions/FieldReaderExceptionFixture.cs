using Shouldly;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Exceptions;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Exceptions;

public class FieldReaderExceptionFixture
{
    private const string DefaultMessage = "The Field could not be read as the type {0}";

    [Theory]
    [AutoNSubstituteData]
    public void Ctor_WithMessage_SetsMessage(string message)
    {
        // Act
        FieldReaderException sut = new(message);

        // Assert
        sut.Message.ShouldBe(message);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Ctor_WithMessageAndException_SetsMessageAndInnerException(
        string message,
        Exception exception)
    {
        // Act
        FieldReaderException sut = new(message, exception);

        // Assert
        sut.Message.ShouldBe(message);
        sut.InnerException.ShouldBe(exception);
    }

    [Fact]
    public void Ctor_WithType_UsesDefaultMessage()
    {
        // Act
        Type type = typeof(int);
        FieldReaderException sut = new(type);

        // Assert
        sut.Message.ShouldBe(string.Format(System.Globalization.CultureInfo.CurrentCulture, DefaultMessage, type));
    }

    [Theory]
    [AutoNSubstituteData]
    public void Ctor_WithException_SetsInnerException(
        Exception exception)
    {
        // Act
        Type type = typeof(int);
        FieldReaderException sut = new(type, exception);

        // Assert
        sut.Message.ShouldBe(string.Format(System.Globalization.CultureInfo.CurrentCulture, DefaultMessage, type));
        sut.InnerException.ShouldBe(exception);
    }
}
