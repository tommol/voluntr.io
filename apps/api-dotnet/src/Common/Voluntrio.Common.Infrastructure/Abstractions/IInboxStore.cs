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
}
