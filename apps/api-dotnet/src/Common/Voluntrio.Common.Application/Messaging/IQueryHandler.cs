namespace Voluntrio.Common.Application.Messaging;

/// <summary>
/// Handles a query and produces a response.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResponse">The type of response produced by the query.</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles a query instance.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query handling response.</returns>
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}
