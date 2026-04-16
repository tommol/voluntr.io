namespace Voluntrio.Common.Application.Abstractions;

/// <summary>
/// Provides persistence for idempotency keys.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Determines whether the given key has already been processed.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> when the key exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the given key as processed.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task MarkProcessedAsync(string key, CancellationToken cancellationToken);
}
