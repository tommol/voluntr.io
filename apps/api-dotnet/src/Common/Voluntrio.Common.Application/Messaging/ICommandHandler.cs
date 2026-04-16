namespace Voluntrio.Common.Application.Messaging;

/// <summary>
/// Handles a command and produces a response.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResponse">The type of response produced by the command.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Handles a command instance.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The command handling response.</returns>
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}
