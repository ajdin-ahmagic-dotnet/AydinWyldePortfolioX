using AydinWyldePortfolioX.Services;

namespace AydinWyldePortfolioX.Middleware
{
    public class VisitorTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly HashSet<string> ExcludedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", 
            ".woff", ".woff2", ".ttf", ".eot", ".map", ".json"
        };

        private static readonly HashSet<string> ExcludedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/api/", "/admin/", "/_", "/lib/", "/css/", "/js/", "/images/", "/fonts/"
        };

        public VisitorTrackingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IVisitorTrackingService trackingService)
        {
            var path = context.Request.Path.Value ?? "";
            
            // Skip tracking for static files and API calls
            if (!ShouldTrack(path))
            {
                await _next(context);
                return;
            }

            // Track the visit
            try
            {
                trackingService.TrackVisit(context, path);
            }
            catch
            {
                // Don't let tracking failures affect the request
            }

            await _next(context);
        }

        private bool ShouldTrack(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            // Check for excluded file extensions
            var extension = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(extension) && ExcludedExtensions.Contains(extension))
            {
                return false;
            }

            // Check for excluded paths
            foreach (var excludedPath in ExcludedPaths)
            {
                if (path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public static class VisitorTrackingMiddlewareExtensions
    {
        public static IApplicationBuilder UseVisitorTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<VisitorTrackingMiddleware>();
        }
    }
}
