using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Organizations.Domain.Events;

/// <summary>
/// Raised when a new member is added to an organization.
/// </summary>
/// <param name="OrganizationId">The ID of the organization.</param>
/// <param name="UserId">The user ID of the member added.</param>
/// <param name="OccurredOnUtc">The UTC timestamp when the member was added.</param>
public sealed record MemberAddedDomainEvent(
    Guid OrganizationId,
    Guid UserId,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
