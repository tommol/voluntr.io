namespace Voluntrio.Organizations.Contracts.Shared;

public sealed record SubOrganizationDto(
    Guid OrganizationId,
    string Name,
    string Slug,
    string Status,
    Guid OwnerUserId,
    Guid ParentOrganizationId,
    DateTime CreatedAtUtc);
