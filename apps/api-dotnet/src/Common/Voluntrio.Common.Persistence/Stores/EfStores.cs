using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Infrastructure.Abstractions;
using Voluntrio.Common.Infrastructure.Persistence;
using Voluntrio.Common.Persistence.Context;

namespace Voluntrio.Common.Persistence.Stores;

public sealed class EfOutboxStore(AppDbContext dbContext) : IOutboxStore
{
    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        dbContext.OutboxMessages.Add(message);
        return Task.CompletedTask;
    }
}

public sealed class EfInboxStore(AppDbContext dbContext) : IInboxStore
{
    public Task AddAsync(InboxMessage message, CancellationToken cancellationToken)
    {
        dbContext.InboxMessages.Add(message);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken)
    {
        return dbContext.InboxMessages.AnyAsync(x => x.Id == messageId, cancellationToken);
    }
}

public sealed class EfIdempotencyStore(AppDbContext dbContext) : IIdempotencyStore
{
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken)
    {
        return dbContext.IdempotencyKeys.AnyAsync(x => x.Key == key, cancellationToken);
    }

    public Task MarkProcessedAsync(string key, CancellationToken cancellationToken)
    {
        dbContext.IdempotencyKeys.Add(new IdempotencyKey
        {
            Key = key,
            ProcessedAtUtc = DateTime.UtcNow
        });

        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
