using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Organizations.Domain.Events;

/// <summary>
/// Raised when a member is removed from an organization.
/// </summary>
/// <param name="OrganizationId">The ID of the organization.</param>
/// <param name="UserId">The user ID of the member removed.</param>
/// <param name="OccurredOnUtc">The UTC timestamp when the member was removed.</param>
public sealed record MemberRemovedDomainEvent(
    Guid OrganizationId,
    Guid UserId,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
