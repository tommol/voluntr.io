using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Organizations.Domain.Events;

public sealed record MemberRemovedDomainEvent(
    Guid OrganizationId,
    Guid UserId,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
