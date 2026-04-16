using Microsoft.Extensions.Logging;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;

namespace Voluntrio.Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior that logs command execution and results.
/// </summary>
/// <typeparam name="TCommand">The type of command.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public sealed class LoggingCommandBehavior<TCommand, TResponse>(ILogger<LoggingCommandBehavior<TCommand, TResponse>> logger)
    : ICommandPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Logs command execution and delegates to the next behavior in the pipeline.
    /// </summary>
    /// <param name="command">The command being executed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <returns>The command response.</returns>
    public async Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<TResponse> next)
    {
        var commandName = typeof(TCommand).Name;
        logger.LogInformation("Handling command {CommandName}", commandName);

        var response = await next();

        if (response is Result result && result.IsFailure)
        {
            logger.LogWarning("Command {CommandName} failed with {Code}: {Message}", commandName, result.Error.Code, result.Error.Message);
        }
        else
        {
            logger.LogInformation("Handled command {CommandName}", commandName);
        }

        return response;
    }
}
