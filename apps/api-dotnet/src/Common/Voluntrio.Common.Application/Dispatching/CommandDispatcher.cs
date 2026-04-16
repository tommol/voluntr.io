using Microsoft.Extensions.DependencyInjection;
using Voluntrio.Common.Application.Messaging;

namespace Voluntrio.Common.Application.Dispatching;

/// <summary>
/// Dispatches commands to their registered handlers with optional pipeline behaviors.
/// </summary>
public sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    /// <summary>
    /// Sends a command through the dispatch pipeline to its handler.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to send.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The command response.</returns>
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
