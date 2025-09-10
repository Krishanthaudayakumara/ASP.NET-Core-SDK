using Sitecore.AspNetCore.SDK.GraphQL.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.Pages.Configuration;
using Sitecore.AspNetCore.SDK.Pages.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting().AddMvc();

builder.Services.AddGraphQLClient(c => c.ContextId = TestConstants.ContextId);

builder.Services.AddSitecoreLayoutService()
                .AddSitecorePagesHandler()
                .AddGraphQLWithContextHandler("default", TestConstants.ContextId!, siteName: TestConstants.SiteName!)
                .AsDefaultHandler();

builder.Services.AddSitecoreRenderingEngine(o => o.AddDefaultPartialView("_ComponentNotFound"))
                .WithSitecorePages(TestConstants.ContextId, o => o.EditingSecret = TestConstants.JssEditingSecret);

WebApplication app = builder.Build();

// Execute optional ITestBootstrapper implementations registered by fixtures.
using (IServiceScope scope = app.Services.CreateScope())
{
    var bootstrappers = scope.ServiceProvider.GetServices<Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.TestBootstrapping.ITestBootstrapper>().ToList();

    foreach (var b in bootstrappers)
    {
        try
        {
            b.ConfigureServices(builder.Services);
        }
        catch
        {
            // best-effort; ignore bootstrapper failures in test program
        }
    }

    foreach (var b in bootstrappers)
    {
        try
        {
            b.Configure(app);
        }
        catch
        {
            // best-effort; ignore bootstrapper failures in test program
        }
    }
}

app.UseSitecorePages(new PagesOptions { ConfigEndpoint = TestConstants.ConfigRoute });
app.UseRouting();

app.MapControllerRoute(name: "default", pattern: "{controller=Pages}/{action=Index}");

app.Run();

// Marker for WebApplicationFactory targeting
public partial class TestPagesProgram
{
    // Intentionally empty; WebApplicationFactory targets this program type in tests.
}