using Microsoft.Extensions.Logging;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;

namespace Voluntrio.Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior that logs query execution and results.
/// </summary>
/// <typeparam name="TQuery">The type of query.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public sealed class LoggingQueryBehavior<TQuery, TResponse>(ILogger<LoggingQueryBehavior<TQuery, TResponse>> logger)
    : IQueryPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Logs query execution and delegates to the next behavior in the pipeline.
    /// </summary>
    /// <param name="query">The query being executed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <returns>The query response.</returns>
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
