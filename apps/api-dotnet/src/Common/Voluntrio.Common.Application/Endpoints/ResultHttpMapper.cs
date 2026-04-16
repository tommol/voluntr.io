using Microsoft.AspNetCore.Http;
using Voluntrio.Common.Application.Results;

namespace Voluntrio.Common.Application.Endpoints;

public static class ResultHttpMapper
{
    public static IResult ToHttpResult(this Error error)
    {
        if (error.Code.StartsWith("auth.", StringComparison.OrdinalIgnoreCase))
        {
            return Microsoft.AspNetCore.Http.Results.Unauthorized();
        }

        if (error.Code.Contains("not_found", StringComparison.OrdinalIgnoreCase))
        {
            return Microsoft.AspNetCore.Http.Results.NotFound(new { error.Code, error.Message });
        }

        if (error.Code.StartsWith("idempotency.", StringComparison.OrdinalIgnoreCase))
        {
            return Microsoft.AspNetCore.Http.Results.Conflict(new { error.Code, error.Message });
        }

        return Microsoft.AspNetCore.Http.Results.BadRequest(new { error.Code, error.Message });
    }
}
