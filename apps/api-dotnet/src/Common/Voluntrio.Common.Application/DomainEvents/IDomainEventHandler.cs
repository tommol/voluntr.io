using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Common.Application.DomainEvents;

public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
