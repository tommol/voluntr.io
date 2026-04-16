using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Organizations.Domain.Events;

/// <summary>
/// Raised when a member's role is changed within an organization.
/// </summary>
/// <param name="OrganizationId">The ID of the organization.</param>
/// <param name="UserId">The user ID of the member whose role changed.</param>
/// <param name="Role">The new role assigned to the member.</param>
/// <param name="OccurredOnUtc">The UTC timestamp when the role was changed.</param>
public sealed record MemberRoleChangedDomainEvent(
    Guid OrganizationId,
    Guid UserId,
    string Role,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
