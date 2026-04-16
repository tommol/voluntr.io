using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;

namespace Voluntrio.Common.Persistence.Context;

public sealed class EfUnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
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
