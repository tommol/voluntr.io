using Microsoft.Extensions.DependencyInjection;
using Voluntrio.Common.Application.Messaging;

namespace Voluntrio.Common.Application.Dispatching;

/// <summary>
/// Dispatches queries to their registered handlers with optional pipeline behaviors.
/// </summary>
public sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    /// <summary>
    /// Sends a query through the dispatch pipeline to its handler.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to send.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
    /// <param name="query">The query to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query response.</returns>
    public Task<TResponse> Send<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        var behaviors = serviceProvider.GetServices<IQueryPipelineBehavior<TQuery, TResponse>>().ToArray();

        QueryHandlerDelegate<TResponse> pipeline = () => handler.Handle(query, cancellationToken);

        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var next = pipeline;
            pipeline = () => behavior.Handle(query, cancellationToken, next);
        }

        return pipeline();
    }
}
