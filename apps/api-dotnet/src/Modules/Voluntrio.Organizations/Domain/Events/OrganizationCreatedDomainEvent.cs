using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Organizations.Domain.Events;

/// <summary>
/// Raised when a new organization is created.
/// </summary>
/// <param name="OrganizationId">The ID of the newly created organization.</param>
/// <param name="OwnerUserId">The user ID of the organization's owner.</param>
/// <param name="OccurredOnUtc">The UTC timestamp when the organization was created.</param>
public sealed record OrganizationCreatedDomainEvent(
    Guid OrganizationId,
    Guid OwnerUserId,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
