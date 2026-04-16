namespace Voluntrio.Organizations.Contracts.UseCases.CreateSubOrganization;

public sealed record CreateSubOrganizationRequest(string Name);

public sealed record CreateSubOrganizationResponse(
    Guid OrganizationId,
    Guid ParentOrganizationId,
    string Name,
    string Slug,
    string Status,
    Guid OwnerUserId,
    DateTime CreatedAtUtc);
