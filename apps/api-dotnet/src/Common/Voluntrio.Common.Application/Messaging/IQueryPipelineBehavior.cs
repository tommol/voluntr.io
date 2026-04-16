namespace Voluntrio.Common.Application.Messaging;

public delegate Task<TResponse> QueryHandlerDelegate<TResponse>();

public interface IQueryPipelineBehavior<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken, QueryHandlerDelegate<TResponse> next);
}
