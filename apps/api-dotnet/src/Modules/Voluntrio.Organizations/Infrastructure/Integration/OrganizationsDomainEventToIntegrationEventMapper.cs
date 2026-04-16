using Voluntrio.Common.Application.Integration;
using Voluntrio.Common.Domain.Abstractions;
using Voluntrio.Organizations.Contracts.IntegrationEvents;
using Voluntrio.Organizations.Domain.Events;

namespace Voluntrio.Organizations.Infrastructure.Integration;

public sealed class OrganizationsDomainEventToIntegrationEventMapper : IDomainEventToIntegrationEventMapper
{
    public bool CanMap(IDomainEvent domainEvent)
    {
        return domainEvent is
            OrganizationCreatedDomainEvent or
            SubOrganizationCreatedDomainEvent or
            MemberAddedDomainEvent or
            MemberRoleChangedDomainEvent or
            MemberRemovedDomainEvent;
    }

    public IReadOnlyCollection<IIntegrationEvent> Map(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            OrganizationCreatedDomainEvent e =>
            [
                new OrganizationCreatedIntegrationEvent(
                    Guid.CreateVersion7(),
                    e.OccurredOnUtc,
                    e.OrganizationId,
                    e.OwnerUserId)
            ],

            SubOrganizationCreatedDomainEvent e =>
            [
                new SubOrganizationCreatedIntegrationEvent(
                    Guid.CreateVersion7(),
                    e.OccurredOnUtc,
                    e.OrganizationId,
                    e.ParentOrganizationId,
                    e.OwnerUserId)
            ],

            MemberAddedDomainEvent e =>
            [
                new OrganizationMemberAddedIntegrationEvent(
                    Guid.CreateVersion7(),
                    e.OccurredOnUtc,
                    e.OrganizationId,
                    e.UserId)
            ],

            MemberRoleChangedDomainEvent e =>
            [
                new OrganizationMemberRoleChangedIntegrationEvent(
                    Guid.CreateVersion7(),
                    e.OccurredOnUtc,
                    e.OrganizationId,
                    e.UserId,
                    e.Role)
            ],

            MemberRemovedDomainEvent e =>
            [
                new OrganizationMemberRemovedIntegrationEvent(
                    Guid.CreateVersion7(),
                    e.OccurredOnUtc,
                    e.OrganizationId,
                    e.UserId)
            ],

            _ => []
        };
    }
}
