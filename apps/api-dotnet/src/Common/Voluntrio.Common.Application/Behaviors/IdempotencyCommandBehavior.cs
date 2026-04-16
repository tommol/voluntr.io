using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;

namespace Voluntrio.Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior that ensures idempotent command handling using idempotency keys.
/// </summary>
/// <typeparam name="TCommand">The type of command.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public sealed class IdempotencyCommandBehavior<TCommand, TResponse>(IIdempotencyStore idempotencyStore)
    : ICommandPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Executes the command with idempotency protection if an idempotency key is present.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <returns>The command response, or a failure if the idempotency key was already processed.</returns>
    public async Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<TResponse> next)
    {
        if (command is not IIdempotentCommand<TResponse> idempotentCommand ||
            string.IsNullOrWhiteSpace(idempotentCommand.IdempotencyKey))
        {
            return await next();
        }

        var key = idempotentCommand.IdempotencyKey!;

        if (await idempotencyStore.ExistsAsync(key, cancellationToken))
        {
            return CreateFailureResponse("idempotency.duplicate", "Command with this idempotency key has already been processed.");
        }

        var response = await next();

        if (response is Result result && result.IsSuccess)
        {
            await idempotencyStore.MarkProcessedAsync(key, cancellationToken);
        }

        return response;
    }

    private static TResponse CreateFailureResponse(string code, string message)
    {
        var error = new Error(code, message);

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = typeof(TResponse).GetMethod(nameof(Result<object>.Failure), [typeof(Error)]);

            if (failureMethod is not null)
            {
                return (TResponse)failureMethod.Invoke(null, [error])!;
            }
        }

        throw new InvalidOperationException($"Idempotency behavior supports only Result responses, received {typeof(TResponse).Name}.");
    }
}
