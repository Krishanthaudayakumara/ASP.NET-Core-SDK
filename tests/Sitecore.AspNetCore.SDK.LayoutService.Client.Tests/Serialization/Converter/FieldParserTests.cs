using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoFixture;
using Shouldly;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization.Converter;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization.Fields;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Serialization.Converter;

public class FieldParserTests
{
    private readonly FieldParser _sut = new();

    // ReSharper disable once UnusedMember.Global - Used by testing framework
    public static Action<IFixture> AutoSetup => f =>
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            Converters = { new FieldConverter(), new PlaceholderFeatureConverter(new FieldParser()) }
        };
        f.Inject(options);
    };

    [Fact]
    public void ParseFields_JsonNotObject_ShouldWrapAsCustomContent()
    {
        // Arrange
        const string json = "[]";
        byte[] bytes = [.. Encoding.UTF8.GetBytes(json)];
        Utf8JsonReader reader = new(bytes);
        reader.Read();

        // Act
        Dictionary<string, IFieldReader> result = _sut.ParseFields(ref reader);

        // Assert
        result.Count.ShouldBe(1);
        (string key, IFieldReader value) = result.First();
        key.ShouldBe(FieldParser.CustomContentFieldKey);
        value.ShouldBeOfType<JsonSerializedField>();
    }

    [Fact]
    public void ParseFields_CorrectJson_ShouldReturnFields()
    {
        // Arrange
        const string json = "{\"value\": 100}";
        byte[] bytes = [.. Encoding.UTF8.GetBytes(json)];
        Utf8JsonReader reader = new(bytes);
        reader.Read();

        // Act
        Dictionary<string, IFieldReader> result = _sut.ParseFields(ref reader);

        // Assert
        result.Count.ShouldBe(1);
        (string key, IFieldReader value) = result.First();
        key.ShouldBe("value");
        value.ShouldBeOfType<JsonSerializedField>();
    }
}
