using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Infrastructure.Abstractions;
using Voluntrio.Common.Infrastructure.Persistence;
using Voluntrio.Common.Persistence.Context;

namespace Voluntrio.Common.Persistence.Stores;

/// <summary>
/// Entity Framework implementation of outbox message storage.
/// </summary>
public sealed class EfOutboxStore(AppDbContext dbContext) : IOutboxStore
{
    /// <summary>
    /// Adds an outbox message to be persisted.
    /// </summary>
    /// <param name="message">The outbox message to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        dbContext.OutboxMessages.Add(message);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Fetches unprocessed outbox messages for a given kind and type prefix.
    /// </summary>
    public async Task<IReadOnlyList<OutboxMessage>> FetchUnprocessedBatchAsync(
        string kind,
        string typePrefix,
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await dbContext.OutboxMessages
            .AsNoTracking()
            .Where(x => x.ProcessedOnUtc == null && x.Kind == kind && x.Type.StartsWith(typePrefix))
            .OrderBy(x => x.OccurredOnUtc)
            .ThenBy(x => x.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Marks an outbox message as processed.
    /// </summary>
    public async Task MarkProcessedAsync(Guid messageId, DateTime processedOnUtc, CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages.SingleOrDefaultAsync(x => x.Id == messageId, cancellationToken);
        if (message is null)
        {
            return;
        }

        message.ProcessedOnUtc = processedOnUtc;
        message.Error = null;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Marks an outbox message as failed.
    /// </summary>
    public async Task MarkFailedAsync(Guid messageId, string error, CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages.SingleOrDefaultAsync(x => x.Id == messageId, cancellationToken);
        if (message is null)
        {
            return;
        }

        message.Error = error.Length <= 2000 ? error : error[..2000];
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Entity Framework implementation of inbox message storage.
/// </summary>
public sealed class EfInboxStore(AppDbContext dbContext) : IInboxStore
{
    /// <summary>
    /// Adds an inbox message to be persisted.
    /// </summary>
    /// <param name="message">The inbox message to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task AddAsync(InboxMessage message, CancellationToken cancellationToken)
    {
        dbContext.InboxMessages.Add(message);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks whether an inbox message with the specified identifier has been received.
    /// </summary>
    /// <param name="messageId">The identifier of the message to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the message exists; otherwise, <c>false</c>.</returns>
    public Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken)
    {
        return dbContext.InboxMessages.AnyAsync(x => x.Id == messageId, cancellationToken);
    }
}

/// <summary>
/// Entity Framework implementation of idempotency key storage.
/// </summary>
public sealed class EfIdempotencyStore(AppDbContext dbContext) : IIdempotencyStore
{
    /// <summary>
    /// Checks whether an idempotency key has already been processed.
    /// </summary>
    /// <param name="key">The idempotency key to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the key has been processed; otherwise, <c>false</c>.</returns>
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken)
    {
        return dbContext.IdempotencyKeys.AnyAsync(x => x.Key == key, cancellationToken);
    }

    /// <summary>
    /// Marks an idempotency key as processed.
    /// </summary>
    /// <param name="key">The idempotency key to mark as processed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
