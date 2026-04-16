using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Common.Application.DomainEvents;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
