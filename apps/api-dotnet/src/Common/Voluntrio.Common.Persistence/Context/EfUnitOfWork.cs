using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;

namespace Voluntrio.Common.Persistence.Context;

/// <summary>
/// Entity Framework implementation of the unit of work pattern using database transactions.
/// </summary>
public sealed class EfUnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    /// <summary>
    /// Executes an operation within a database transaction.
    /// </summary>
    /// <typeparam name="TResponse">The type of response returned by the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="shouldCommit">A predicate that determines whether to commit based on the operation's response.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation response.</returns>
    public async Task<TResponse> ExecuteInTransactionAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> operation,
        Func<TResponse, bool> shouldCommit,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is not null)
        {
            return await operation(cancellationToken);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var response = await operation(cancellationToken);
        if (shouldCommit(response))
        {
            await transaction.CommitAsync(cancellationToken);
        }
        else
        {
            await transaction.RollbackAsync(cancellationToken);
        }

        return response;
    }
}
