using Voluntrio.Common.Infrastructure.Persistence;

namespace Voluntrio.Common.Infrastructure.Abstractions;

/// <summary>
/// Provides persistence operations for outbox messages.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Adds a message to the outbox.
    /// </summary>
    /// <param name="message">The message to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches an ordered batch of unprocessed outbox messages.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> FetchUnprocessedBatchAsync(
        string kind,
        string typePrefix,
        int batchSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marks a message as successfully processed.
    /// </summary>
    Task MarkProcessedAsync(Guid messageId, DateTime processedOnUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a message as failed and schedules another retry.
    /// </summary>
    Task MarkFailedForRetryAsync(
        Guid messageId,
        string error,
        int retryCount,
        DateTime nextRetryOnUtc,
        CancellationToken cancellationToken);

    /// <summary>
    /// Moves a message to dead-letter state after retry exhaustion.
    /// </summary>
    Task MarkDeadLetterAsync(
        Guid messageId,
        string error,
        int retryCount,
        DateTime deadLetteredOnUtc,
        CancellationToken cancellationToken);
}
