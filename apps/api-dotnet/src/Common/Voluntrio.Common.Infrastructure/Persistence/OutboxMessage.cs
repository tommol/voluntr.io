namespace Voluntrio.Common.Infrastructure.Persistence;

/// <summary>
/// Represents a message published to the outbox for external systems to process.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for the outbox message.
    /// </summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>
    /// Gets or sets the UTC timestamp when the domain event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; init; }

    /// <summary>
    /// Gets or sets the logical message kind (for example: domain, integration).
    /// </summary>
    public string Kind { get; init; } = "integration";

    /// <summary>
    /// Gets or sets the fully qualified name of the domain event type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized payload of the domain event.
    /// </summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the outbox message was processed and published.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets any error message that occurred during message processing.
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
