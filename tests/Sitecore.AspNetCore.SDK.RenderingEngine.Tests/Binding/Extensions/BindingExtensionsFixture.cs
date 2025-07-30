using Shouldly;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Sitecore.AspNetCore.SDK.RenderingEngine.Binding.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Binding.Providers;
using Sitecore.AspNetCore.SDK.RenderingEngine.Tests.Mocks;
using Xunit;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Tests.Binding.Extensions;

public class BindingExtensionsFixture
{
    [Fact]
    public void GetModelBinder_WithTSourceAndTType_IsGuarded()
    {
        // Arrange
        Func<BinderTypeModelBinder?> act =
            () => BindingExtensions.GetModelBinder<TestSitecoreLayoutBindingSource, object>(null!);

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually;
    }

    [Fact]
    public void GetModelBinder_WithTSource_IsGuarded()
    {
        // Arrange
        Func<BinderTypeModelBinder?> act =
            () => BindingExtensions.GetModelBinder<TestSitecoreLayoutBindingSource>(null!);

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually;
    }

    [Fact]
    public void AddSitecoreModelBinderProviders_IsGuarded()
    {
        // Arrange
        Action act =
            () => BindingExtensions.AddSitecoreModelBinderProviders(null!);

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually;
    }

    [Fact]
    public void GetModelBinder_WithValidBindingSource_ReturnsCorrectModelBinder()
    {
        // Arrange
        TestModelMetadata testMetadata = new(typeof(object));
        BindingInfo bindingInfo = new()
        {
            BindingSource = new TestSitecoreLayoutBindingSource()
        };
        TestModelBinderProviderContext context = new(testMetadata, bindingInfo);

        // Act
        BinderTypeModelBinder? result = context.GetModelBinder<TestSitecoreLayoutBindingSource>();

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public void GetModelBinder_WithInvalidBindingSource_ReturnsNull()
    {
        // Arrange
        TestModelMetadata testMetadata = new(typeof(object));
        BindingInfo bindingInfo = new()
        {
            BindingSource = new TestBindingSource()
        };
        TestModelBinderProviderContext context = new(testMetadata, bindingInfo);

        // Act
        BinderTypeModelBinder? result = context.GetModelBinder<TestSitecoreLayoutBindingSource>();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetModelBinder_WithValidModelTypeAndBindingSource_ReturnsCorrectModelBinder()
    {
        // Arrange
        TestModelMetadata testMetadata = new(typeof(string));
        BindingInfo bindingInfo = new()
        {
            BindingSource = new TestSitecoreLayoutBindingSource()
        };
        TestModelBinderProviderContext context = new(testMetadata, bindingInfo);

        // Act
        BinderTypeModelBinder? result = context.GetModelBinder<TestSitecoreLayoutBindingSource, string>();

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public void GetModelBinder_WithInvalidModelTypeAndBindingSource_ReturnsNull()
    {
        // Arrange
        TestModelMetadata testMetadata = new(typeof(object));
        BindingInfo bindingInfo = new()
        {
            BindingSource = new TestBindingSource()
        };
        TestModelBinderProviderContext context = new(testMetadata, bindingInfo);

        // Act
        BinderTypeModelBinder? result = context.GetModelBinder<TestSitecoreLayoutBindingSource, string>();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void AddSitecoreModelBinderProviders_MvcOptions_Contain_ExpectedProviders()
    {
        // Arrange
        MvcOptions options = new();

        // Act
        options.AddSitecoreModelBinderProviders();

        // Assert
        options.ModelBinderProviders.Count.ShouldBe(4);
        options.ModelBinderProviders[0].ShouldBeOfType<SitecoreLayoutRouteModelBinderProvider>();
        options.ModelBinderProviders[1].ShouldBeOfType<SitecoreLayoutContextModelBinderProvider>();
        options.ModelBinderProviders[2].ShouldBeOfType<SitecoreLayoutComponentModelBinderProvider>();
        options.ModelBinderProviders[3].ShouldBeOfType<SitecoreLayoutResponseModelBinderProvider>();
    }
}
