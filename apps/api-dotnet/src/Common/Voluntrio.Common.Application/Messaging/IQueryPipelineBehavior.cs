namespace Voluntrio.Common.Application.Messaging;

public delegate Task<TResponse> QueryHandlerDelegate<TResponse>();

/// <summary>
/// Defines a middleware behavior for query handling pipelines.
/// </summary>
/// <typeparam name="TQuery">The type of query in the pipeline.</typeparam>
/// <typeparam name="TResponse">The type of response in the pipeline.</typeparam>
public interface IQueryPipelineBehavior<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles a query and optionally invokes the next behavior in the pipeline.
    /// </summary>
    /// <param name="query">The query being handled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <returns>The pipeline response.</returns>
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken, QueryHandlerDelegate<TResponse> next);
}
