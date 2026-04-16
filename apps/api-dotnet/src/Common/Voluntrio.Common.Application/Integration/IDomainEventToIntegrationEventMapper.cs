using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Common.Application.Integration;

/// <summary>
/// Maps a domain event into one or more integration events.
/// </summary>
public interface IDomainEventToIntegrationEventMapper
{
    bool CanMap(IDomainEvent domainEvent);
    IReadOnlyCollection<IIntegrationEvent> Map(IDomainEvent domainEvent);
}
