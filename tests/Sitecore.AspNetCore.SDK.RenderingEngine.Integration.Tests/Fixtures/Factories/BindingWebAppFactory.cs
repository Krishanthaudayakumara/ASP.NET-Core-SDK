using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Factories;

public class BindingWebAppFactory : TestWebApplicationFactory<TestBindingProgram>
{
    public BindingWebAppFactory()
    {
    }
}
