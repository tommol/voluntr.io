using Voluntrio.Common.Application.Results;

namespace Voluntrio.Organizations.Application.Shared;

internal static class EndpointUserContext
{
    private const string UserHeaderName = "X-User-Id";
    private const string IdempotencyHeaderName = "X-Idempotency-Key";

    public static Result<Guid> ReadActorUserId(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue(UserHeaderName, out var values))
        {
            return Result<Guid>.Failure(new Error("auth.user_missing", $"Missing '{UserHeaderName}' header."));
        }

        if (!Guid.TryParse(values.ToString(), out var actorUserId) || actorUserId == Guid.Empty)
        {
            return Result<Guid>.Failure(new Error("auth.user_invalid", $"Header '{UserHeaderName}' must contain a valid GUID."));
        }

        return Result<Guid>.Success(actorUserId);
    }

    public static string? ReadIdempotencyKey(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue(IdempotencyHeaderName, out var values))
        {
            return null;
        }

        var key = values.ToString().Trim();
        return string.IsNullOrWhiteSpace(key) ? null : key;
    }
}
