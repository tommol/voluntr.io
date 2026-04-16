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
    /// Marks a message as failed during processing.
    /// </summary>
    Task MarkFailedAsync(Guid messageId, string error, CancellationToken cancellationToken);
}
