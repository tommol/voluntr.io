using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Organizations.Domain.Events;

public sealed record MemberAddedDomainEvent(
    Guid OrganizationId,
    Guid UserId,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
