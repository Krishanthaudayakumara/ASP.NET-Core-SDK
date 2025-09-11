using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Factories;

public class PagesWebAppFactory : TestWebApplicationFactory<TestPagesProgram>
{
    // Intentionally minimal; extend in future to customize test services/pipeline
    public PagesWebAppFactory()
    {
    }
}
