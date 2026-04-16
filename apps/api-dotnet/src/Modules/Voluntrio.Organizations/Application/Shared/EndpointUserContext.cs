using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Voluntrio.Organizations.Application.Shared;

internal static class EndpointUserContext
{
    private const string IdempotencyHeaderName = "X-Idempotency-Key";

    public static string? ReadIdempotencyKey(HttpContext httpContext)
    {
        var logger = GetLogger(httpContext);

        if (!httpContext.Request.Headers.TryGetValue(IdempotencyHeaderName, out var values))
        {
            return null;
        }

        var key = values.ToString().Trim();

        if (string.IsNullOrWhiteSpace(key))
        {
            logger?.LogWarning(
                "Header {HeaderName} was provided but empty for {Method} {Path}",
                IdempotencyHeaderName,
                httpContext.Request.Method,
                httpContext.Request.Path);

            return null;
        }

        return key;
    }

    private static ILogger? GetLogger(HttpContext httpContext)
    {
        var loggerFactory = httpContext.RequestServices.GetService<ILoggerFactory>();
        return loggerFactory?.CreateLogger("EndpointUserContext");
    }
}
