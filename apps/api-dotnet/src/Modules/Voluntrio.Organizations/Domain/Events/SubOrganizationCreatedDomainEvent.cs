using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Organizations.Domain.Events;

/// <summary>
/// Raised when a new sub-organization is created under a parent organization.
/// </summary>
/// <param name="OrganizationId">The ID of the newly created sub-organization.</param>
/// <param name="ParentOrganizationId">The ID of the parent organization.</param>
/// <param name="OwnerUserId">The user ID of the sub-organization owner.</param>
/// <param name="OccurredOnUtc">The UTC timestamp when the sub-organization was created.</param>
public sealed record SubOrganizationCreatedDomainEvent(
    Guid OrganizationId,
    Guid ParentOrganizationId,
    Guid OwnerUserId,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
