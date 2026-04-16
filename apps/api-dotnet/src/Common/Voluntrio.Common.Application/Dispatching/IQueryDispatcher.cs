using Voluntrio.Common.Application.Messaging;

namespace Voluntrio.Common.Application.Dispatching;

public interface IQueryDispatcher
{
    Task<TResponse> Send<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>;
}
