using Microsoft.Extensions.DependencyInjection;
using Voluntrio.Common.Application.Messaging;

namespace Voluntrio.Common.Application.Dispatching;

public sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
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
