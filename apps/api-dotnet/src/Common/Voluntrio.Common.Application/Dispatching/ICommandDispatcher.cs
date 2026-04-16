using Voluntrio.Common.Application.Messaging;

namespace Voluntrio.Common.Application.Dispatching;

/// <summary>
/// Dispatches commands to their corresponding handlers.
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Sends a command to its handler.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="command">The command instance to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The command response.</returns>
    Task<TResponse> Send<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>;
}
