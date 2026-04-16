namespace Voluntrio.Common.Infrastructure.Persistence;

/// <summary>
/// Represents an idempotency key used to track processed operations.
/// </summary>
public sealed class IdempotencyKey
{
    /// <summary>
    /// Gets or sets the unique idempotency key string.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the operation was marked as processed.
    /// </summary>
    public DateTime ProcessedAtUtc { get; init; }
}
