using Voluntrio.Common.Infrastructure.Persistence;

namespace Voluntrio.Common.Infrastructure.Abstractions;

public interface IOutboxStore
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);
}
