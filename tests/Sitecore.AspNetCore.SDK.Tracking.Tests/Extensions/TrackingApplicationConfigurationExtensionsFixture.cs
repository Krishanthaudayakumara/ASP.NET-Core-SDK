using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using Shouldly;
using NSubstitute;
using Xunit;

namespace Sitecore.AspNetCore.SDK.Tracking.Tests.Extensions;

public class TrackingApplicationConfigurationExtensionsFixture
{
    // ReSharper disable once UnusedMember.Global - Used by testing framework
    [ExcludeFromCodeCoverage]
    public static Action<IFixture> AutoSetup => f =>
    {
        IServiceProvider? services = Substitute.For<IServiceProvider>();

        f.Inject(services);
    };

    [Fact]
    public void AddSitecoreTracking_NullServices_Throws()
    {
        // Arrange
        Action action = () => TrackingAppConfigurationExtensions.WithTracking(null!);

        // Act / Assert
        var ex = Should.Throw<ArgumentNullException>(() => action()); // TODO: Assert exception properties manually
            // TODO: Split assertion chain manuallyParamName.ShouldBe("serviceBuilder");
    }
}
