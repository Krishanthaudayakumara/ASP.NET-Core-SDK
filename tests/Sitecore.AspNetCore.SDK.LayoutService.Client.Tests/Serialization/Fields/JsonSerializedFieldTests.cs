using System.Text.Json;
using System.Text.Json.Serialization;
using AutoFixture;
using AutoFixture.Idioms;
using Shouldly;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.AutoFixture.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Exceptions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization.Fields;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Serialization.Fields;

public class JsonSerializedFieldTests
{
    // ReSharper disable once UnusedMember.Global - Used by testing framework
    public static Action<IFixture> AutoSetup => f =>
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            Converters = { new JsonStringEnumConverter() }
        };
        f.Inject(options);

        string json = "{\"value\": 100}";
        JsonDocument token = JsonDocument.Parse(json);
        f.Inject(token);
    };

    [Theory]
    [AutoNSubstituteData]
    public void Ctor_IsGuarded(GuardClauseAssertion guard)
    {
        guard.VerifyConstructors<JsonSerializedField>();
    }

    [Fact]
    public void Read_ValidJson_Deserializes()
    {
        // Arrange
        string json = "{\"value\": 100}";
        JsonDocument token = JsonDocument.Parse(json);
        JsonSerializedField sut = new(token);

        // Act
        ValueField<int>? result = sut.Read<ValueField<int>>();

        // Assert
        result!.Value.ShouldBe(100);
    }

    [Fact]
    public void Read_ValidJson_DeserializesMultipleTimes()
    {
        // Arrange
        const string json = "{\"value\": 100}";
        JsonDocument token = JsonDocument.Parse(json);
        JsonSerializedField sut = new(token);
        Action action = () =>
        {
            sut.Read<ValueField<int>>();
            sut.Read<ValueField<int>>();
            sut.Read<ValueField<int>>();
            sut.Read<ValueField<int>>();
            sut.Read<ValueField<int>>();
        };

        // Act / Assert
        Should.NotThrow(() => action());
    }

    [Fact]
    public void Read_InvalidJson_ThrowsException()
    {
        // Arrange
        const string json = "[{\"value\": 100}]";
        JsonDocument token = JsonDocument.Parse(json);
        JsonSerializedField sut = new(token);
        Action action = () => sut.Read<ValueField<int>>();

        // Act / Assert
        var ex = Should.Throw<FieldReaderException>(() => action()); // TODO: Assert exception properties manually
            .WithInnerException<JsonException>();
    }

    [Fact]
    public void TryRead_ValidJson_Deserializes()
    {
        // Arrange
        const string json = "{\"value\": 100}";
        JsonDocument token = JsonDocument.Parse(json);
        JsonSerializedField sut = new(token);

        // Act
        bool result = sut.TryRead(typeof(ValueField<int>), out IField? field);

        // Assert
        result.ShouldBeTrue();
        field.Should().BeOfType<ValueField<int>>();
        ((ValueField<int>)field!).Value.ShouldBe(100);
    }

    [Fact]
    public void TryRead_InvalidJson_ThrowsException()
    {
        // Arrange
        const string json = "[{\"value\": 100}]";
        JsonDocument token = JsonDocument.Parse(json);
        JsonSerializedField sut = new(token);

        // Act / Assert
        bool result = sut.TryRead(typeof(ValueField<int>), out IField? field);

        // Assert
        result.ShouldBeFalse();
        field.ShouldBeNull();
    }

    [Fact]
    public void GetRawValue_ReturnsExpectedJson()
    {
        // Arrange
        const string json = "{\"value\": 100}";
        JsonDocument token = JsonDocument.Parse(json);
        JsonSerializedField sut = new(token);

        // Act
        string result = sut.GetRawValue();

        // Assert
        result.ShouldBe(json);
    }

    private class ValueField<T> : IField
    {
        public required T Value { get; set; }
    }
}
