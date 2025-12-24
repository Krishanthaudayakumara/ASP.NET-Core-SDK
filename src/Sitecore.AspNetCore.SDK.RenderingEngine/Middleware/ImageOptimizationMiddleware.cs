using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Middleware;

/// <summary>
/// Middleware that provides Next.js-style image optimization for ASP.NET Core applications.
/// Handles image resizing, format conversion, and compression via the /_sitecore/image endpoint.
/// </summary>
public class ImageOptimizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ImageOptimizationMiddleware> _logger;
    private readonly Dictionary<string, string> _formatMimeTypes = new()
    {
        { "jpeg", "image/jpeg" },
        { "jpg", "image/jpeg" },
        { "webp", "image/webp" },
        { "png", "image/png" }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageOptimizationMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public ImageOptimizationMiddleware(RequestDelegate next, ILogger<ImageOptimizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Handles the HTTP request and processes image optimization if the request path matches the image endpoint.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the completion of request processing.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/_sitecore/image"))
        {
            await ProcessImageOptimization(context);
            return;
        }

        await _next(context);
    }

    private static async Task SaveImageWithFormat(Image image, MemoryStream output, string format, int quality)
    {
        switch (format.ToLower())
        {
            case "webp":
                WebpEncoder webpEncoder = new WebpEncoder { Quality = quality };
                await image.SaveAsync(output, webpEncoder);
                break;
            case "png":
                await image.SaveAsPngAsync(output);
                break;
            case "jpeg":
            case "jpg":
            default:
                JpegEncoder jpegEncoder = new JpegEncoder { Quality = quality };
                await image.SaveAsync(output, jpegEncoder);
                break;
        }
    }

    private async Task ProcessImageOptimization(HttpContext context)
    {
        try
        {
            // Parse query parameters
            IQueryCollection query = context.Request.Query;
            string? imageUrl = query["url"].FirstOrDefault();
            int width = int.TryParse(query["w"], out int w) ? w : 0;
            int quality = int.TryParse(query["q"], out int q) ? q : 75;
            string format = query["format"].FirstOrDefault() ?? "jpeg";

            if (string.IsNullOrEmpty(imageUrl) || width <= 0)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Missing or invalid parameters");
                return;
            }

            // Decode the URL
            string decodedUrl = HttpUtility.UrlDecode(imageUrl);

            // Get cache key
            string cacheKey = $"img_{decodedUrl}_{width}_{quality}_{format}".GetHashCode().ToString();

            // Check if image is already cached (you can implement caching here)
            // For now, we'll process the image every time

            // Fetch the original image
            using HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            byte[] imageBytes = await httpClient.GetByteArrayAsync(decodedUrl);

            // Process the image
            using Image image = Image.Load(imageBytes);
            using MemoryStream output = new MemoryStream();

            // Calculate aspect ratio and resize
            double aspectRatio = (double)image.Height / image.Width;
            int newHeight = (int)(width * aspectRatio);

            // Resize image while maintaining aspect ratio
            image.Mutate(x => x.Resize(width, newHeight));

            // Save with appropriate format and compression
            await SaveImageWithFormat(image, output, format, quality);

            // Set response headers
            string mimeType = _formatMimeTypes.GetValueOrDefault(format.ToLower(), "image/jpeg");
            context.Response.ContentType = mimeType;
            context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
            context.Response.Headers.Append("Vary", "Accept");

            // Write optimized image to response
            byte[] optimizedBytes = output.ToArray();
            await context.Response.Body.WriteAsync(optimizedBytes);

            _logger.LogInformation(
                "Optimized image: {Url} -> {Width}x{Height} ({Format}, {Quality}%, {Size} bytes)", decodedUrl, width, newHeight, format, quality, optimizedBytes.Length);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching image from URL: {Url}", context.Request.Query["url"].FirstOrDefault() ?? string.Empty);
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Image not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image optimization for URL: {Url}", context.Request.Query["url"].FirstOrDefault() ?? string.Empty);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }
}
