namespace Voluntrio.Common.Application.Messaging;

public delegate Task<TResponse> CommandHandlerDelegate<TResponse>();

/// <summary>
/// Defines a middleware behavior for command handling pipelines.
/// </summary>
/// <typeparam name="TCommand">The type of command in the pipeline.</typeparam>
/// <typeparam name="TResponse">The type of response in the pipeline.</typeparam>
public interface ICommandPipelineBehavior<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Handles a command and optionally invokes the next behavior in the pipeline.
    /// </summary>
    /// <param name="command">The command being handled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <returns>The pipeline response.</returns>
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<TResponse> next);
}
