using Shouldly;
using Sitecore.AspNetCore.SDK.ExperienceEditor.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Interfaces;
using Xunit;

namespace Sitecore.AspNetCore.SDK.ExperienceEditor.Tests.Extensions;

public class ServiceCollectionExtensionsFixture
{
    [Fact]
    public void AddSitecoreRenderingEngine_IsGuarded()
    {
        Func<ISitecoreRenderingEngineBuilder> act =
            () => ExperienceEditorAppConfigurationExtensions.WithExperienceEditor(null!);

        var ex = Should.Throw<ArgumentNullException>(() => act()); // TODO: Assert exception properties manually;
    }
}
