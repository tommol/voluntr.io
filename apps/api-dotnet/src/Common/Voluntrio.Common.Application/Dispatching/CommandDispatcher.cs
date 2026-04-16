using Microsoft.Extensions.DependencyInjection;
using Voluntrio.Common.Application.Messaging;

namespace Voluntrio.Common.Application.Dispatching;

public sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public Task<TResponse> Send<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        var behaviors = serviceProvider.GetServices<ICommandPipelineBehavior<TCommand, TResponse>>().ToArray();

        CommandHandlerDelegate<TResponse> pipeline = () => handler.Handle(command, cancellationToken);

        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var next = pipeline;
            pipeline = () => behavior.Handle(command, cancellationToken, next);
        }

        return pipeline();
    }
}
