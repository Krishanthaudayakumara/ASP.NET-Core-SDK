using AutoFixture;
using Shouldly;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sitecore.AspNetCore.SDK.AutoFixture.Attributes;
using Sitecore.AspNetCore.SDK.RenderingEngine.Configuration;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Rendering;
using Sitecore.AspNetCore.SDK.RenderingEngine.Tests.Mocks;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Tests.Extensions;

public class RenderingEngineOptionsExtensionsFixture
{
    // ReSharper disable once UnusedMember.Global - Used by testing framework
    public static Action<IFixture> AutoSetup => f =>
    {
        ILogger<LoggingComponentRenderer>? logger = Substitute.For<ILogger<LoggingComponentRenderer>>();
        ICustomHtmlHelper? customHtmlHelper = f.Create<ICustomHtmlHelper>();

        IServiceProvider? services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(ILogger<LoggingComponentRenderer>)).Returns(logger);
        services.GetService(typeof(IHtmlHelper)).Returns(customHtmlHelper);
        f.Inject(services);

        f.Inject(new ViewContext());
        ICustomViewComponentHelper? viewComponentHelper = f.Create<ICustomViewComponentHelper>();
        f.Inject<IViewComponentHelper>(viewComponentHelper);
        f.Inject<IViewContextAware>(viewComponentHelper);

        IServiceProvider? serviceProvider = f.Freeze<IServiceProvider>();
        serviceProvider.GetService(typeof(IViewComponentHelper)).Returns(viewComponentHelper);

        RenderingEngineOptions options = new();
        f.Inject(options);
    };

    [Fact]
    public void AddPartialView_NullOptions_Throws()
    {
        // Arrange
        Action action = () => RenderingEngineOptionsExtensions.AddPartialView(null!, (Predicate<string>)null!, null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("options");
    }

    [Theory]
    [MemberAutoNSubstituteData(nameof(EmptyStrings))]
    public void AddPartialView_InvalidLayoutComponentName_Throws(string value, RenderingEngineOptions options)
    {
        // Arrange
        Action action = () => options.AddPartialView(value, null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentException>(() => action()); // TODO: Assert exception properties manually;
    }

    [Theory]
    [MemberAutoNSubstituteData(nameof(EmptyStrings))]
    public void AddPartialView_EmptyPartialViewPath_Throws(string value, RenderingEngineOptions options, string layoutComponentName)
    {
        // Arrange
        Action action = () => options.AddPartialView(layoutComponentName, value);

        // Act / Assert
        var ex = Should.Throw<ArgumentException>(() => action()); // TODO: Assert exception properties manually// TODO: Assert exception.ParamName manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddPartialView_PartialViewPathIsNotEmpty_MatchesOnPartialFileName(
        RenderingEngineOptions options)
    {
        // Act
        options.AddPartialView("~/foo/Bar.cshtml");

        // Act / Assert
        options.RendererRegistry.Values.ShouldNotBeEmpty();

        ComponentRendererDescriptor descriptor = options.RendererRegistry.Values[0];
        descriptor.Match("Bar").ShouldBeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddPartialView_PartialViewPathIsNotEmpty_PartialViewComponentRendererIsAddedToOptions(
        RenderingEngineOptions options,
        IServiceProvider services,
        string layoutComponentName,
        string partialViewPath)
    {
        // Act
        options.AddPartialView(layoutComponentName, partialViewPath);

        // Act / Assert
        options.RendererRegistry.Values.ShouldNotBeEmpty();

        ComponentRendererDescriptor descriptor = options.RendererRegistry.Values[0];
        descriptor.ShouldNotBeNull();

        IComponentRenderer renderer = descriptor.GetOrCreate(services);
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PartialViewComponentRenderer>();
    }

    [Fact]
    public void AddViewComponent_NullOptions_Throws()
    {
        // Arrange
        Action action = () => RenderingEngineOptionsExtensions.AddViewComponent(null!, (Predicate<string>)null!, null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("options");
    }

    [Theory]
    [MemberAutoNSubstituteData(nameof(EmptyStrings))]
    public void AddViewComponent_InvalidLayoutComponentName_Throws(string value, RenderingEngineOptions options)
    {
        // Arrange
        Action action = () => options.AddViewComponent(value, null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("layoutComponentName");
    }

    [Theory]
    [MemberAutoNSubstituteData(nameof(EmptyStrings))]
    public void AddViewComponent_EmptyPartialViewPath_Throws(string value, RenderingEngineOptions options, string layoutComponentName)
    {
        // Arrange
        Action action = () => options.AddViewComponent(layoutComponentName, value);

        // Act / Assert
        var ex = Should.Throw<ArgumentException>(() => action()); // TODO: Assert exception properties manually;
    }

    [Fact]
    public void AddModelBoundViewOfTModel_OptionsIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        Action action = () => RenderingEngineOptionsExtensions.AddModelBoundView<ContentBlock>(null!, (Predicate<string>)null!, null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("options");
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddModelBoundViewOfTModel_PredicateIsNull_ThrowsArgumentNullException(RenderingEngineOptions options)
    {
        // Arrange
        Action action = () => options.AddModelBoundView<ContentBlock>((Predicate<string>)null!, null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("match");
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddModelBoundViewOfTModel_ViewComponentNameIsNull_ThrowsArgumentNullException(RenderingEngineOptions options, Predicate<string> match)
    {
        // Arrange
        Action action = () => options.AddModelBoundView<ContentBlock>(match, null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("viewName");
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddModelBoundViewOfTModel_ViewComponentNameIsEmptyString_ThrowsArgumentNullException(RenderingEngineOptions options, Predicate<string> match)
    {
        // Arrange
        Action action = () => options.AddModelBoundView<ContentBlock>(match, string.Empty);

        // Act / Assert
        var ex = Should.Throw<ArgumentException>(() => action()); // TODO: Assert exception properties manually// TODO: Assert exception.ParamName manually;
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddModelBoundViewOfTModel_PartialViewPathIsNotEmpty_MatchesOnPartialFileName(
        RenderingEngineOptions options)
    {
        // Act
        options.AddModelBoundView<ContentBlock>("~/foo/Bar.cshtml");

        // Act / Assert
        options.RendererRegistry.Values.ShouldNotBeEmpty();

        ComponentRendererDescriptor descriptor = options.RendererRegistry.Values[0];
        descriptor.Match("Bar").ShouldBeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddModelBoundViewOfTModel_ValidParameters_OptionsRendererRegistryContainsSingleComponentRendererDescriptor(RenderingEngineOptions options, Predicate<string> match, string viewComponentName)
    {
        // Arrange
        options.RendererRegistry.Clear();

        // Act
        options = options.AddModelBoundView<ContentBlock>(match, viewComponentName);

        // Assert
        options.RendererRegistry.Count.ShouldBe(1);
        options.RendererRegistry[0].ShouldBeOfType<ComponentRendererDescriptor>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddModelBoundViewOfTModel_ValidParametersRendererRegistryIsNotEmpty_NewComponentRendererDescriptorIsAddedToTheEndOfTheList(RenderingEngineOptions options, string viewComponentName)
    {
        // Arrange
        string componentName = "InitialComponent";
        ComponentRendererDescriptor initialDescriptor = new(name => name == componentName, _ => null!, componentName);
        options.RendererRegistry.Add(0, initialDescriptor);

        // Act
        options = options.AddModelBoundView<ContentBlock>(name => name == "TestComponent", viewComponentName);

        // Assert
        options.RendererRegistry.Count.ShouldBe(2);
        options.RendererRegistry[1].ShouldBeOfType<ComponentRendererDescriptor>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddModelBoundViewOfTModel_ValidParameters_ComponentRendererDescriptorCreatesModelBoundViewComponentComponentRenderer(IServiceProvider services, RenderingEngineOptions options, Predicate<string> match, string viewComponentName)
    {
        // Arrange
        options.RendererRegistry.Clear();

        // Act
        options = options.AddModelBoundView<ContentBlock>(match, viewComponentName);

        // Assert
        ComponentRendererDescriptor descriptor = options.RendererRegistry[0];
        IComponentRenderer renderer = descriptor.GetOrCreate(services);
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<ModelBoundViewComponentComponentRenderer<ContentBlock>>();
    }

    [Fact]
    public void AddDefaultComponentRenderer_NullOptions_Throws()
    {
        // Arrange
        Action action = () => RenderingEngineOptionsExtensions.AddDefaultComponentRenderer(null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("options");
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddDefaultComponentRenderer_OptionsIsNotNull_DefaultRendererIsLoggingComponentRenderer(RenderingEngineOptions options, IServiceProvider services)
    {
        // Act
        options.AddDefaultComponentRenderer();

        // Act / Assert
        ComponentRendererDescriptor? descriptor = options.DefaultRenderer;
        descriptor.ShouldNotBeNull();

        IComponentRenderer renderer = descriptor!.GetOrCreate(services);
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<LoggingComponentRenderer>();
    }

    [Fact]
    public void AddDefaultComponentRendererOfT_NullOptions_Throws()
    {
        // Arrange
        Action action = () => RenderingEngineOptionsExtensions.AddDefaultComponentRenderer<TestComponentRenderer>(null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("options");
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddDefaultComponentRendererOfT_GenericTypeParameterIsTestComponentRenderer_DefaultRendererIsTestComponentRenderer(RenderingEngineOptions options, IServiceProvider services)
    {
        // Act
        options.AddDefaultComponentRenderer<TestComponentRenderer>();

        // Act / Assert
        ComponentRendererDescriptor? descriptor = options.DefaultRenderer;
        descriptor.ShouldNotBeNull();

        IComponentRenderer renderer = descriptor!.GetOrCreate(services);
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<TestComponentRenderer>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void AddPostRenderingAction_NullOptions_Throws(RenderingEngineOptions options)
    {
        // Arrange
        Action action = () => RenderingEngineOptionsExtensions.AddPostRenderingAction(null!, null!);
        Action action2 = () => options.AddPostRenderingAction(null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("options");
        var ex = Should.Throw<ArgumentNullException>(() => action2()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("postAction");
    }

    private static IEnumerable<object[]> EmptyStrings()
    {
        yield return [null!];
        yield return [string.Empty];
        yield return ["\t\t   "];
    }
}
