using Voluntrio.Common.Infrastructure.Persistence;

namespace Voluntrio.Common.Infrastructure.Abstractions;

/// <summary>
/// Provides persistence operations for inbox messages.
/// </summary>
public interface IInboxStore
{
    /// <summary>
    /// Adds a message to the inbox.
    /// </summary>
    /// <param name="message">The message to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(InboxMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a message with the given identifier exists in the inbox.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> when the message exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches an ordered batch of unprocessed inbox messages for a given type prefix.
    /// </summary>
    Task<IReadOnlyList<InboxMessage>> FetchUnprocessedBatchAsync(
        string typePrefix,
        int batchSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marks a message as processed.
    /// </summary>
    Task MarkProcessedAsync(Guid messageId, DateTime processedAtUtc, CancellationToken cancellationToken);

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
