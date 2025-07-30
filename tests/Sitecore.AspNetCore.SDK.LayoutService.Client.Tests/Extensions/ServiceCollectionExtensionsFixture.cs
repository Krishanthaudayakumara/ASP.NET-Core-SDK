using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Interfaces;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Serialization;
using Xunit;

namespace Sitecore.AspNetCore.SDK.LayoutService.Client.Tests.Extensions;

public class ServiceCollectionExtensionsFixture
{
    [Fact]
    public void AddSitecoreLayoutService_ServiceCollectionIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        Func<ISitecoreLayoutClientBuilder> act =
            () => ServiceCollectionExtensions.AddSitecoreLayoutService(null!);

        // Act & Assert
        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually// TODO: Assert exception.Message manually");
    }

    [Fact]
    public void AddSitecoreLayoutService_ServiceCollection_Contains_ExpectedServices()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        services.AddSitecoreLayoutService();

        // Assert
        services.ShouldContain(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ISitecoreLayoutClient) && serviceDescriptor.Lifetime == ServiceLifetime.Transient);
        services.ShouldContain(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ISitecoreLayoutSerializer) && serviceDescriptor.Lifetime == ServiceLifetime.Singleton);
    }
}
