using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;

namespace Voluntrio.Common.Application.Behaviors;

public sealed class TransactionCommandBehavior<TCommand, TResponse>(IUnitOfWork unitOfWork)
    : ICommandPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
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
