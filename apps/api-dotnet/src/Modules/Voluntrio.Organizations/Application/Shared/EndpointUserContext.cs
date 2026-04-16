using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Voluntrio.Common.Application.Results;

namespace Voluntrio.Organizations.Application.Shared;

internal static class EndpointUserContext
{
    private const string UserHeaderName = "X-User-Id";
    private const string IdempotencyHeaderName = "X-Idempotency-Key";

    public static Result<Guid>   ReadActorUserId(HttpContext httpContext)
    {
        var logger = GetLogger(httpContext);

        if (!httpContext.Request.Headers.TryGetValue(UserHeaderName, out var values))
        {
            logger?.LogWarning(
                "Missing required header {HeaderName} for {Method} {Path}",
                UserHeaderName,
                httpContext.Request.Method,
                httpContext.Request.Path);

            return Result<Guid>.Failure(new Error("auth.user_missing", $"Missing '{UserHeaderName}' header."));
        }

        if (!Guid.TryParse(values.ToString(), out var actorUserId) || actorUserId == Guid.Empty)
        {
            logger?.LogWarning(
                "Invalid header {HeaderName} value for {Method} {Path}",
                UserHeaderName,
                httpContext.Request.Method,
                httpContext.Request.Path);

            return Result<Guid>.Failure(new Error("auth.user_invalid", $"Header '{UserHeaderName}' must contain a valid GUID."));
        }

        return Result<Guid>.Success(actorUserId);
    }

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
