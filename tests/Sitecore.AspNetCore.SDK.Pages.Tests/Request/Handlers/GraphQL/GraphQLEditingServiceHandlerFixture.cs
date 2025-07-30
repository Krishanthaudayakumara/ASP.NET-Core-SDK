using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.Idioms;
using Shouldly;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.AutoFixture.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Exceptions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Request;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization.Fields;
using Sitecore.AspNetCore.SDK.Pages.Request.Handlers.GraphQL;
using Sitecore.AspNetCore.SDK.Pages.Services;
using Xunit;

namespace Sitecore.AspNetCore.SDK.Pages.Tests.Request.Handlers.GraphQL;

public class GraphQLEditingServiceHandlerFixture
{
    [ExcludeFromCodeCoverage]
    public static Action<IFixture> AutoSetup => f =>
    {
        IGraphQLClient client = Substitute.For<IGraphQLClient>();
        f.Inject(client);

        ISitecoreLayoutSerializer mockSerializer = Substitute.For<ISitecoreLayoutSerializer>();
        f.Inject(mockSerializer);

        IDictionaryService mockDictionaryService = Substitute.For<IDictionaryService>();
        f.Inject(mockDictionaryService);

        SitecoreLayoutRequest request = new()
        {
            {
                "sc_request_headers_key", new Dictionary<string, string[]>()
                {
                    { "mode", ["edit"] },
                    { "language", ["en"] },
                    { "sc_layoutKind", ["Final"] },
                    { "sc_itemid", ["item_1234"] },
                    { "sc_version", ["version_1234"] }
                }
            },
            {
                "sc_lang", "en"
            },
            {
                "sc_site", "site_1234"
            }
        };
        f.Inject(request);
    };

    [Theory]
    [AutoNSubstituteData]
    public void Ctor_InvalidArgs_Throws(GuardClauseAssertion guard)
    {
        guard.VerifyConstructors<GraphQLEditingServiceHandler>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_RequestParamIsNull_ErrorThrown(GraphQLEditingServiceHandler sut)
    {
        // Act
        Func<Task> act = async () => { await sut.Request(null!, string.Empty); };

        // Assert
        var ex = await Should.ThrowAsync<ArgumentNullException>(act); // TODO: Assert exception properties manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_HandlerNameIsNull_ErrorThrown(GraphQLEditingServiceHandler sut)
    {
        // Act
        Func<Task> act = async () => { await sut.Request([], null!); };

        // Assert
        var ex = await Should.ThrowAsync<ArgumentException>(act); // TODO: Assert exception properties manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_HandlerNameIsEmptyString_ErrorThrown(GraphQLEditingServiceHandler sut)
    {
        // Act
        Func<Task> act = async () => { await sut.Request([], string.Empty); };

        // Assert
        var ex = await Should.ThrowAsync<ArgumentException>(act); // TODO: Assert exception properties manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_NotValidEditingRequest_ErrorThrown(GraphQLEditingServiceHandler sut)
    {
        // Arrange
        SitecoreLayoutRequest request = [];

        // Act
        Func<Task> act = async () => { await sut.Request(request, "editingHandler"); };

        // Assert
        var ex = await Should.ThrowAsync<ArgumentException>(act); // TODO: Assert exception properties manually// TODO: Assert exception.Message manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_NoLanguageSet_ErrorThrown(GraphQLEditingServiceHandler sut)
    {
        // Arrange
        SitecoreLayoutRequest request = new()
        {
            {
                "sc_request_headers_key", new Dictionary<string, string[]>()
                {
                    { "mode", ["edit"] }
                }
            }
        };

        // Act
        SitecoreLayoutResponse result = await sut.Request(request, "editingHandler");

        // Assert
        result.Errors.All(x => x is ItemNotFoundSitecoreLayoutServiceClientException).ShouldBeTrue() // TODO: Check type safety manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_ValidRequest_NoErrorsThrown(IGraphQLClient client, SitecoreLayoutRequest request)
    {
        // Arrange
        client.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLRequest>()).Returns(Constants.SimpleEditingLayoutQueryResponse);
        GraphQLEditingServiceHandler sut = new(client, Substitute.For<ISitecoreLayoutSerializer>(), Substitute.For<ILogger<GraphQLEditingServiceHandler>>(), Substitute.For<IDictionaryService>());

        // Act
        SitecoreLayoutResponse result = await sut.Request(request, "editingHandler");

        // Assert
        result.Errors.ShouldBeEmpty();
        await client.Received(1).SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLRequest>());
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_ValidRequest_DictionaryServiceIsCalled(IGraphQLClient client, SitecoreLayoutRequest request, IDictionaryService mockDictionaryService)
    {
        // Arrange
        client.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLRequest>()).Returns(Constants.EditingLayoutQueryResponseWithDictionaryPaging);

        GraphQLEditingServiceHandler sut = new(client, Substitute.For<ISitecoreLayoutSerializer>(), Substitute.For<ILogger<GraphQLEditingServiceHandler>>(), mockDictionaryService);

        // Act
        SitecoreLayoutResponse result = await sut.Request(request, "editingHandler");

        // Asset
        result.Errors.ShouldBeEmpty();
        await client.Received(1).SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLRequest>());
        await mockDictionaryService.Received(1).GetSiteDictionary(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IGraphQLClient>());
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_ValidRequest_PlaceholderChromesAreAdded(IGraphQLClient client, ISitecoreLayoutSerializer mockSerializer, SitecoreLayoutRequest request)
    {
        // Arrange
        client.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLRequest>()).Returns(Constants.MockEditingLayoutQueryResponse);
        mockSerializer.Deserialize(Arg.Any<string>()).Returns(Constants.MockLayoutResponse_Placeholder);
        GraphQLEditingServiceHandler sut = new(client, mockSerializer, Substitute.For<ILogger<GraphQLEditingServiceHandler>>(), Substitute.For<IDictionaryService>());

        // Act
        SitecoreLayoutResponse result = await sut.Request(request, "editingHandler");

        // Assert
        result.Errors.ShouldBeEmpty();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"].Count.ShouldBe(2);
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][0].ShouldBeOfType<EditableChrome>();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][0].As<EditableChrome>().Attributes["chrometype"].ShouldBe("placeholder");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][0].As<EditableChrome>().Attributes["kind"].ShouldBe("open");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][0].As<EditableChrome>().Attributes["id"].ShouldBe($"placeholder_1_{Guid.Empty.ToString()}");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][1].ShouldBeOfType<EditableChrome>();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][1].As<EditableChrome>().Attributes["chrometype"].ShouldBe("placeholder");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][1].As<EditableChrome>().Attributes["kind"].ShouldBe("close");
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_ValidRequest_NestedPlaceholderChromesAreAdded(IGraphQLClient client, ISitecoreLayoutSerializer mockSerializer, SitecoreLayoutRequest request)
    {
        // Arrange
        client.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLRequest>()).Returns(Constants.MockEditingLayoutQueryResponse);
        mockSerializer.Deserialize(Arg.Any<string>()).Returns(Constants.MockLayoutResponse_NestedPlaceholder);
        GraphQLEditingServiceHandler sut = new(client, mockSerializer, Substitute.For<ILogger<GraphQLEditingServiceHandler>>(), Substitute.For<IDictionaryService>());

        // Act
        SitecoreLayoutResponse result = await sut.Request(request, "editingHandler");

        // Assert
        result.Errors.ShouldBeEmpty();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][0].ShouldBeOfType<EditableChrome>();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][0].As<EditableChrome>().Attributes["chrometype"].ShouldBe("placeholder");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][0].As<EditableChrome>().Attributes["kind"].ShouldBe("open");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][0].As<EditableChrome>().Attributes["id"].ShouldBe("nested_placeholder_1_component_1");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][1].ShouldBeOfType<EditableChrome>();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][1].As<EditableChrome>().Attributes["chrometype"].ShouldBe("placeholder");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][1].As<EditableChrome>().Attributes["kind"].ShouldBe("close");
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_ValidRequest_RenderingChromesAreAdded(IGraphQLClient client, ISitecoreLayoutSerializer mockSerializer, SitecoreLayoutRequest request)
    {
        // Arrange
        client.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLRequest>()).Returns(Constants.MockEditingLayoutQueryResponse);
        mockSerializer.Deserialize(Arg.Any<string>()).Returns(Constants.MockLayoutResponse_WithComponentInPlaceholder);
        GraphQLEditingServiceHandler sut = new(client, mockSerializer, Substitute.For<ILogger<GraphQLEditingServiceHandler>>(), Substitute.For<IDictionaryService>());

        // Act
        SitecoreLayoutResponse result = await sut.Request(request, "editingHandler");

        // Assert
        result.Errors.ShouldBeEmpty();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"].Count.ShouldBe(5);
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][1].ShouldBeOfType<EditableChrome>();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][1].As<EditableChrome>().Attributes["chrometype"].ShouldBe("rendering");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][1].As<EditableChrome>().Attributes["kind"].ShouldBe("open");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][1].As<EditableChrome>().Attributes["id"].ShouldBe($"component_1");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][3].ShouldBeOfType<EditableChrome>();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][3].As<EditableChrome>().Attributes["chrometype"].ShouldBe("rendering");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][3].As<EditableChrome>().Attributes["kind"].ShouldBe("close");
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_ValidRequest_RenderingInNestedPlaceholderChromesAreAdded(IGraphQLClient client, ISitecoreLayoutSerializer mockSerializer, SitecoreLayoutRequest request)
    {
        // Arrange
        client.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLRequest>()).Returns(Constants.MockEditingLayoutQueryResponse);
        mockSerializer.Deserialize(Arg.Any<string>()).Returns(Constants.MockLayoutResponse_ComponentInNestedPlaceholder);
        GraphQLEditingServiceHandler sut = new(client, mockSerializer, Substitute.For<ILogger<GraphQLEditingServiceHandler>>(), Substitute.For<IDictionaryService>());

        // Act
        SitecoreLayoutResponse result = await sut.Request(request, "editingHandler");

        // Assert
        result.Errors.ShouldBeEmpty();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"].Count.ShouldBe(5);
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][1].ShouldBeOfType<EditableChrome>();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][1].As<EditableChrome>().Attributes["chrometype"].ShouldBe("rendering");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][1].As<EditableChrome>().Attributes["kind"].ShouldBe("open");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][1].As<EditableChrome>().Attributes["id"].ShouldBe($"nested_component_2");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][3].ShouldBeOfType<EditableChrome>();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][3].As<EditableChrome>().Attributes["chrometype"].ShouldBe("rendering");
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Placeholders["nested_placeholder_1"][3].As<EditableChrome>().Attributes["kind"].ShouldBe("close");
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Request_ValidRequest_FieldRenderingChromesAreAdded(IGraphQLClient client, ISitecoreLayoutSerializer mockSerializer, SitecoreLayoutRequest request)
    {
        // Arrange
        client.SendQueryAsync<EditingLayoutQueryResponse>(Arg.Any<GraphQLRequest>()).Returns(Constants.MockEditingLayoutQueryResponse);
        mockSerializer.Deserialize(Arg.Any<string>()).Returns(Constants.MockLayoutResponse_ComponentWithField);
        GraphQLEditingServiceHandler sut = new(client, mockSerializer, Substitute.For<ILogger<GraphQLEditingServiceHandler>>(), Substitute.For<IDictionaryService>());

        // Act
        SitecoreLayoutResponse result = await sut.Request(request, "editingHandler");

        // Assert
        result.Errors.ShouldBeEmpty();
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"].Count.ShouldBe(5);
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Fields.Values.Count.ShouldBe(1);
        result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Fields["field_1"].ShouldBeOfType<JsonSerializedField>();
        JsonSerializedField? jsonSerialisedField = result?.Content?.Sitecore?.Route?.Placeholders["placeholder_1"][2].As<Component>().Fields["field_1"] as JsonSerializedField;
        jsonSerialisedField.ShouldNotBeNull();
        EditableField<object>? editableField = jsonSerialisedField?.Read<EditableField<object>>();
        editableField.ShouldNotBeNull();
        editableField?.OpeningChrome.ShouldNotBeNull();
        editableField?.OpeningChrome?.Attributes["chrometype"].ShouldBe("field");
        editableField?.OpeningChrome?.Attributes["kind"].ShouldBe("open");
        editableField?.OpeningChrome?.Content.ShouldBe(@"{""datasource"":{""id"":""datasource_id"",""language"":""en"",""revision"":""revision_1"",""version"":1},""title"":""Text"",""fieldId"":""field_id"",""fieldType"":""Text"",""rawValue"":""field_raw_value""}");
        editableField?.ClosingChrome.ShouldNotBeNull();
        editableField?.ClosingChrome?.Attributes["chrometype"].ShouldBe("field");
        editableField?.ClosingChrome?.Attributes["kind"].ShouldBe("close");
    }
}
