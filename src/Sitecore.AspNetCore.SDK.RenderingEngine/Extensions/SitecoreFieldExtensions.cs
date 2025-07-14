using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model.Fields;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;

/// <summary>
/// Set of extension methods for Sitecore fields.
/// </summary>
public static partial class SitecoreFieldExtensions
{
    /// <summary>
    /// Gets modified URL string to Sitecore media item.
    /// </summary>
    /// <param name="imageField">The image field.</param>
    /// <param name="imageParams">Image parameters, example: new { mw = 100, mh = 50 }. **IMPORTANT**: All the parameters you pass must be whitelisted for resizing to occur. See /sitecore/config/*.config (search for 'allowedMediaParams').</param>
    /// <returns>Media item URL.</returns>
    public static string? GetMediaLink(this ImageField imageField, object? imageParams)
    {
        ArgumentNullException.ThrowIfNull(imageField);
        string? urlStr = imageField.Value.Src;

        if (urlStr == null)
        {
            return null;
        }

        return GetSitecoreMediaUri(urlStr, imageParams);
    }

    /// <summary>
    /// Gets URL to Sitecore media item.
    /// </summary>
    /// <param name="url">The image URL.</param>
    /// <param name="imageParams">Image parameters.</param>
    /// <returns>Media item URL.</returns>
    private static string GetSitecoreMediaUri(string url, object? imageParams)
    {
        if (imageParams != null)
        {
            // Parse existing query parameters from the original URL
            var originalParams = ParseUrlParameters(url);
            
            // Split URL to get base path
            string[] urlParts = url.Split('?');
            string baseUrl = urlParts[0];
            
            // Merge original parameters with new ones (new ones take precedence)
            RouteValueDictionary newParameters = new(imageParams);
            
            // Start with original parameters
            RouteValueDictionary mergedParams = new();
            foreach (var kvp in originalParams)
            {
                mergedParams[kvp.Key] = kvp.Value;
            }
            
            // Add/override with new parameters
            foreach (string key in newParameters.Keys)
            {
                mergedParams[key] = newParameters[key];
            }
            
            // Rebuild URL with merged parameters
            url = baseUrl;
            foreach (string key in mergedParams.Keys)
            {
                if (mergedParams[key] != null)
                {
                    url = QueryHelpers.AddQueryString(url, key, mergedParams[key]?.ToString() ?? string.Empty);
                }
            }
        }

        // TODO Review hardcoded matching and replacement
        Match match = MediaUrlPrefixRegex().Match(url);
        if (match.Success)
        {
            url = url.Replace(match.Value, $"/{match.Groups[1]}/jssmedia/", StringComparison.InvariantCulture);
        }

        return url;
    }

    /// <summary>
    /// Parses query parameters from a URL into a dictionary.
    /// </summary>
    /// <param name="url">The URL to parse.</param>
    /// <returns>Dictionary of query parameters.</returns>
    private static Dictionary<string, object> ParseUrlParameters(string url)
    {
        var parameters = new Dictionary<string, object>();
        
        if (string.IsNullOrEmpty(url))
        {
            return parameters;
        }
        
        var queryIndex = url.IndexOf('?');
        if (queryIndex == -1)
        {
            return parameters;
        }
        
        var queryString = url.Substring(queryIndex + 1);
        var parsedQuery = QueryHelpers.ParseQuery(queryString);
        
        foreach (var kvp in parsedQuery)
        {
            if (kvp.Value.Count > 0)
            {
                parameters[kvp.Key] = kvp.Value.First() ?? string.Empty;
            }
        }
        
        return parameters;
    }

    [GeneratedRegex("/([-~]{1})/media/")]
    private static partial Regex MediaUrlPrefixRegex();
}