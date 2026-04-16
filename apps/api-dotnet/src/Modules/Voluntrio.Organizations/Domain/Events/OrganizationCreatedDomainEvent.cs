using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Organizations.Domain.Events;

public sealed record OrganizationCreatedDomainEvent(
    Guid OrganizationId,
    Guid OwnerUserId,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
