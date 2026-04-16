using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;

namespace Voluntrio.Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command execution in a database transaction.
/// </summary>
/// <typeparam name="TCommand">The type of command.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public sealed class TransactionCommandBehavior<TCommand, TResponse>(IUnitOfWork unitOfWork)
    : ICommandPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Executes the command within a transactional scope.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <returns>The command response.</returns>
    public Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<TResponse> next)
    {
        return unitOfWork.ExecuteInTransactionAsync(async _ =>
        {
            return await next();
        }, ShouldCommit, cancellationToken);
    }

    private static bool ShouldCommit(TResponse response)
    {
        return response is not Result result || result.IsSuccess;
    }
}
