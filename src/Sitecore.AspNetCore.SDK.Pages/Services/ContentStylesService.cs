using Microsoft.Extensions.Options;
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
    private const string CkContentClassName = "ck-content";
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

        bool stylesRequired = false;
        if (layoutData.Sitecore.Route.Placeholders != null)
        {
            foreach (var placeholder in layoutData.Sitecore.Route.Placeholders.Values)
            {
                foreach (var component in placeholder.Where(c => c is Component).Cast<Component>())
                {
                    if (HasCkContentClass(component))
                    {
                        stylesRequired = true;
                        break;
                    }
                }

                if (stylesRequired)
                {
                    break;
                }
            }
        }

        return stylesRequired ? $"{_pagesOptions.PagesAssetServerUrl}/pages/styles/content-styles.min.css" : null;
    }

    private static bool HasCkContentClass(Component? component)
    {
        if (component == null)
        {
            return false;
        }

        if (component.Fields != null)
        {
            foreach (IFieldReader field in component.Fields.Values)
            {
                if (IsRichTextWithCkContent(field))
                {
                    return true;
                }
            }
        }

        if (component.Placeholders != null)
        {
            foreach (Placeholder placeholder in component.Placeholders.Values)
            {
                foreach (Component childComponent in placeholder.Where(c => c is Component).Cast<Component>())
                {
                    if (HasCkContentClass(childComponent))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool IsRichTextWithCkContent(IFieldReader fieldReader)
    {
        if (fieldReader is not RichTextField richTextField)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(richTextField.EditableMarkup) && richTextField.EditableMarkup.Contains(CkContentClassName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrEmpty(richTextField.Value) && richTextField.Value.Contains(CkContentClassName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
