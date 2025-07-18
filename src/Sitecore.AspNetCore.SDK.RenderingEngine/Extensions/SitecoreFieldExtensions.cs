using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Response.Model.Fields;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;

/// <summary>
/// Set of extension methods for Sitecore fields.
/// </summary>
public static partial class SitecoreFieldExtensions
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

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
    /// Gets modified URL string to Sitecore media item for srcSet.
    /// This method preserves existing URL parameters and merges them with new ones.
    /// </summary>
    /// <param name="imageField">The image field.</param>
    /// <param name="imageParams">Base image parameters.</param>
    /// <param name="srcSetParams">SrcSet specific parameters that override imageParams.</param>
    /// <returns>Media item URL.</returns>
    public static string? GetMediaLinkForSrcSet(this ImageField imageField, object? imageParams, object? srcSetParams)
    {
        ArgumentNullException.ThrowIfNull(imageField);
        string? urlStr = imageField.Value?.Src;

        if (string.IsNullOrEmpty(urlStr))
        {
            return null;
        }

        Dictionary<string, object?> mergedParams = MergeParameters(imageParams, srcSetParams);
        return GetSitecoreMediaUriWithPreservation(urlStr, mergedParams);
    }

    /// <summary>
    /// Merges base parameters with override parameters.
    /// </summary>
    /// <param name="baseParams">Base parameters.</param>
    /// <param name="overrideParams">Override parameters that take precedence.</param>
    /// <returns>Merged parameters as dictionary.</returns>
    private static Dictionary<string, object?> MergeParameters(object? baseParams, object? overrideParams)
    {
        Dictionary<string, object?> result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        AddParametersToResult(result, baseParams);
        AddParametersToResult(result, overrideParams);

        return result;
    }

    /// <summary>
    /// Adds parameters from an object to the result dictionary.
    /// </summary>
    /// <param name="result">The result dictionary to add parameters to.</param>
    /// <param name="parameters">The parameters object to extract values from.</param>
    private static void AddParametersToResult(Dictionary<string, object?> result, object? parameters)
    {
        if (parameters == null)
        {
            return;
        }

        if (parameters is Dictionary<string, object> dict)
        {
            foreach (KeyValuePair<string, object> kvp in dict)
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        else
        {
            PropertyInfo[] properties = _propertyCache.GetOrAdd(
                parameters.GetType(),
                type => type.GetProperties());

            foreach (PropertyInfo prop in properties)
            {
                object? value = prop.GetValue(parameters);
                if (value != null)
                {
                    result[prop.Name] = value;
                }
            }
        }
    }

    /// <summary>
    /// Gets URL to Sitecore media item.
    /// </summary>
    /// <param name="url">The image URL.</param>
    /// <param name="imageParams">Image parameters.</param>
    /// <returns>Media item URL.</returns>
    private static string GetSitecoreMediaUri(string url, object? imageParams)
    {
        // TODO What's the reason we strip away existing querystring?
        if (imageParams != null)
        {
            int queryIndex = url.IndexOf('?');
            if (queryIndex >= 0)
            {
                url = url.Substring(0, queryIndex);
            }

            RouteValueDictionary parameters = new(imageParams);
            foreach (string key in parameters.Keys)
            {
                url = QueryHelpers.AddQueryString(url, key, parameters[key]?.ToString() ?? string.Empty);
            }
        }

        return ApplyJssMediaUrlPrefix(url);
    }

    /// <summary>
    /// Gets modified URL string to Sitecore media item with parameter preservation.
    /// This method preserves existing URL parameters and merges them with new ones.
    /// </summary>
    /// <param name="urlStr">The URL string.</param>
    /// <param name="parameters">Parameters to merge.</param>
    /// <returns>Modified URL string.</returns>
    private static string GetSitecoreMediaUriWithPreservation(string urlStr, object? parameters)
    {
        if (string.IsNullOrEmpty(urlStr))
        {
            return urlStr;
        }

        // Parse existing query parameters using built-in methods
        Dictionary<string, object?> mergedParams = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        string baseUrl = urlStr;

        if (urlStr.Contains('?'))
        {
            string[] parts = urlStr.Split('?', 2);
            baseUrl = parts[0];
            string queryString = parts[1];

            // Use QueryHelpers for parsing existing parameters
            Dictionary<string, StringValues> existingParams = QueryHelpers.ParseQuery(queryString);
            foreach (KeyValuePair<string, StringValues> kvp in existingParams)
            {
                mergedParams[kvp.Key] = kvp.Value.ToString();
            }
        }

        // Add new parameters (these will override existing ones)
        AddParametersToResult(mergedParams, parameters);

        // Build final URL
        string url = baseUrl;
        foreach (KeyValuePair<string, object?> kvp in mergedParams)
        {
            if (kvp.Value != null)
            {
                url = QueryHelpers.AddQueryString(url, kvp.Key, kvp.Value.ToString() ?? string.Empty);
            }
        }

        return ApplyJssMediaUrlPrefix(url);
    }

    /// <summary>
    /// Applies JSS media URL prefix replacement to the given URL.
    /// </summary>
    /// <param name="url">The URL to transform.</param>
    /// <returns>The URL with JSS media prefix applied if applicable.</returns>
    private static string ApplyJssMediaUrlPrefix(string url)
    {
        // TODO Review hardcoded matching and replacement
        Match match = MediaUrlPrefixRegex().Match(url);
        if (match.Success)
        {
            url = url.Replace(match.Value, $"/{match.Groups[1]}/jssmedia/", StringComparison.InvariantCulture);
        }

        return url;
    }

    [GeneratedRegex("/([-~]{1})/media/")]
    private static partial Regex MediaUrlPrefixRegex();
}