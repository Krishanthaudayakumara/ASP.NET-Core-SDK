using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests
{
    public static class TestFactoryHelper
    {
        public static TestWebApplicationFactory<TestPagesProgram> CreateDefault()
        {
            return new TestWebApplicationFactory<TestPagesProgram>();
        }

        public static TestWebApplicationFactory<TestPagesProgram> CreateWithBasicSetup()
        {
            return new TestWebApplicationFactory<TestPagesProgram>()
                .ConfigureServices(builder =>
                {
                    builder.AddSitecoreLayoutService();
                    builder.AddSitecoreRenderingEngine();
                });
        }
    }
}
