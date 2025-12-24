using Microsoft.AspNetCore.Builder;
using Sitecore.AspNetCore.SDK.RenderingEngine.Middleware;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;

/// <summary>
/// Extension methods for configuring image optimization middleware.
/// </summary>
public static class ImageOptimizationExtensions
{
    /// <summary>
    /// Adds Next.js-style image optimization middleware to the application pipeline.
    /// This middleware handles requests to /_sitecore/image and provides image resizing,
    /// format conversion, and compression capabilities.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseImageOptimization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ImageOptimizationMiddleware>();
    }
}
