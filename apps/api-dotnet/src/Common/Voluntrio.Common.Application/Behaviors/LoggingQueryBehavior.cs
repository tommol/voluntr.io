using Microsoft.Extensions.Logging;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;

namespace Voluntrio.Common.Application.Behaviors;

public sealed class LoggingQueryBehavior<TQuery, TResponse>(ILogger<LoggingQueryBehavior<TQuery, TResponse>> logger)
    : IQueryPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken, QueryHandlerDelegate<TResponse> next)
    {
        var queryName = typeof(TQuery).Name;
        logger.LogInformation("Handling query {QueryName}", queryName);

        var response = await next();

        if (response is Result result && result.IsFailure)
        {
            logger.LogWarning("Query {QueryName} failed with {Code}: {Message}", queryName, result.Error.Code, result.Error.Message);
        }
        else
        {
            logger.LogInformation("Handled query {QueryName}", queryName);
        }

        return response;
    }
}
