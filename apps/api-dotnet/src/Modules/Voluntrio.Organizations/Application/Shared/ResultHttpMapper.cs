using Voluntrio.Common.Application.Results;

namespace Voluntrio.Organizations.Application.Shared;

internal static class ResultHttpMapper
{
    public static IResult ToHttpResult(Error error)
    {
        if (error.Code.StartsWith("auth.", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Unauthorized();
        }

        if (error.Code.Contains("not_found", StringComparison.OrdinalIgnoreCase))
        {
            return Results.NotFound(new { error.Code, error.Message });
        }

        return Results.BadRequest(new { error.Code, error.Message });
    }
}
