using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.TestData;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting()
                .AddMvc();

builder.Services.AddSitecoreLayoutService()
                .AddHttpHandler("mock", _ => new HttpClient { BaseAddress = new Uri("http://layout.service") })
                .AsDefaultHandler();

builder.Services.AddSitecoreRenderingEngine();

WebApplication app = builder.Build();
app.UseSitecoreRenderingEngine();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pages}/{action=Index}");

app.Run();

/// <summary>
/// Partial class allowing this TestProgram to be created by a WebApplicationFactory for integration testing.
/// </summary>
public partial class TestBindingProgram
{
}
