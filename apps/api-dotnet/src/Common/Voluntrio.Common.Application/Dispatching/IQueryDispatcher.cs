using Voluntrio.Common.Application.Messaging;

namespace Voluntrio.Common.Application.Dispatching;

/// <summary>
/// Dispatches queries to their corresponding handlers.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Sends a query to its handler.
    /// </summary>
    /// <typeparam name="TQuery">The query type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="query">The query instance to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query response.</returns>
    Task<TResponse> Send<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>;
}
