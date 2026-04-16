using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Organizations.Domain.Events;

public sealed record MemberRoleChangedDomainEvent(
    Guid OrganizationId,
    Guid UserId,
    string Role,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
