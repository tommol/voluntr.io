namespace Voluntrio.Common.Infrastructure.Persistence;

/// <summary>
/// Represents a message received in the inbox from external systems.
/// </summary>
public sealed class InboxMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for the inbox message.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the message was received.
    /// </summary>
    public DateTime ReceivedAtUtc { get; init; }

    /// <summary>
    /// Gets or sets the fully qualified name of the external event type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized payload of the external event.
    /// </summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the inbox message was processed by the application.
    /// </summary>
    public DateTime? ProcessedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the latest processing error message.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the number of failed processing attempts.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp after which processing may be retried.
    /// </summary>
    public DateTime? NextRetryOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the message was moved to dead-letter state.
    /// </summary>
    public DateTime? DeadLetteredOnUtc { get; set; }
}
