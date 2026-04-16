using Voluntrio.Common.Infrastructure.Persistence;

namespace Voluntrio.Common.Infrastructure.Abstractions;

public interface IInboxStore
{
    Task AddAsync(InboxMessage message, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken);
}
