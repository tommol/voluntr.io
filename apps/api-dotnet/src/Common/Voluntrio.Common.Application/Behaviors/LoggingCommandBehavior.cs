using Microsoft.Extensions.Logging;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;

namespace Voluntrio.Common.Application.Behaviors;

public sealed class LoggingCommandBehavior<TCommand, TResponse>(ILogger<LoggingCommandBehavior<TCommand, TResponse>> logger)
    : ICommandPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
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
