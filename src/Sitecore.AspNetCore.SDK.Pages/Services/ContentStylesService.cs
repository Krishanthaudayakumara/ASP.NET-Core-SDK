using Microsoft.Extensions.Options;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model.Fields;
using Sitecore.AspNetCore.SDK.Pages.Configuration;
using System.Linq;

namespace Sitecore.AspNetCore.SDK.Pages.Services;

/// <summary>
/// Service to detect and provide links for content-related stylesheets, such as for the Rich Text Editor.
/// </summary>
public class ContentStylesService : IContentStylesService
{
    private readonly PagesOptions _pagesOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentStylesService"/> class.
    /// </summary>
    /// <param name="pagesOptions">The pages configuration options.</param>
    public ContentStylesService(IOptions<PagesOptions> pagesOptions)
    {
        ArgumentNullException.ThrowIfNull(pagesOptions);
        _pagesOptions = pagesOptions.Value;
    }

    /// <inheritdoc />
    public virtual string? GetContentStylesheetLink(SitecoreLayoutResponseContent? layoutData)
    {
        if (!_pagesOptions.EnableNewRteEditor || layoutData?.Sitecore?.Route == null)
        {
            return null;
        }

        bool stylesRequired = HasCkContentClass(layoutData.Sitecore.Route);

        return stylesRequired ? $"{_pagesOptions.PagesAssetServerUrl}/pages/styles/content-styles.min.css" : null;
    }

    private static bool HasCkContentClass(IPlaceholderFeature feature)
    {
        return feature is Component component && HasCkContentClass(component);
    }

    private static bool HasCkContentClass(Route? route)
    {
        return route != null && route.Placeholders.Values.Any(p => p.Any(HasCkContentClass));
    }

    private static bool HasCkContentClass(Component? component)
    {
        if (component == null)
        {
            return false;
        }

        // Check current component fields
        if (component.Fields.Any(f => f.Value is RichTextField richTextField && richTextField.Value.Contains("class=\"ck-content\"", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Recursively check placeholders
        return component.Placeholders.Values.Any(p => p.Any(HasCkContentClass));
    }
}
