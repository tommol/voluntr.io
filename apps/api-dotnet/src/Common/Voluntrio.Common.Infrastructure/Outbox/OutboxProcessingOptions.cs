namespace Voluntrio.Common.Infrastructure.Outbox;

/// <summary>
/// Configuration options for outbox background processors.
/// </summary>
public sealed class OutboxProcessingOptions
{
    public const string SectionName = "OutboxProcessing";

    /// <summary>
    /// Maximum number of attempts before a message is dead-lettered.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts for failed messages.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Delay between polling cycles when no messages are available.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(2);
}