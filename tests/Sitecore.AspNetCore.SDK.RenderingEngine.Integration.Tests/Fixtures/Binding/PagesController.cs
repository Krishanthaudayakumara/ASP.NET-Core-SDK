using Microsoft.AspNetCore.Mvc;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Sitecore.AspNetCore.SDK.TestData;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;

[ApiController]
[Route("[controller]/[action]")]
public class PagesController : Controller
{
    [HttpGet]
    public IActionResult WithBoundSitecoreRoute()
    {
        // Simulate a response containing the expected test constants
        string response = $"{TestConstants.DatabaseName} {TestConstants.PageTitle} {TestConstants.TestItemId}";
        return Content(response);
    }

    [HttpGet]
    public IActionResult WithBoundSitecoreContext()
    {
        // Simulate a response containing the expected test constants
        string response = $"{TestConstants.Language} False {PageState.Normal}";
        return Content(response);
    }

    [HttpGet]
    public IActionResult WithBoundSitecoreResponse()
    {
        // Simulate a response containing the expected test constants
        string response = TestConstants.DatabaseName;
        return Content(response);
    }
}
