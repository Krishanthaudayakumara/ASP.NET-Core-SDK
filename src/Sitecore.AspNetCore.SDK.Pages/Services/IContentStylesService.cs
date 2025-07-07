using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;

namespace Sitecore.AspNetCore.SDK.Pages.Services;

/// <summary>
/// Defines the contract for a service that detects and provides links for content-related stylesheets.
/// </summary>
public interface IContentStylesService
{
    /// <summary>
    /// Traverses the layout data and returns a stylesheet URL if content styles (e.g., for the new RTE) are required.
    /// </summary>
    /// <param name="layoutData">The layout service response content to traverse.</param>
    /// <returns>A URL to a stylesheet, or null if no specific styles are required.</returns>
    string? GetContentStylesheetLink(SitecoreLayoutResponseContent? layoutData);
}
