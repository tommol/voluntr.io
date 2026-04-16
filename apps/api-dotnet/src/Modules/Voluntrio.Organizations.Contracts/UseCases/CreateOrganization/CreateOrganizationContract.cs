namespace Voluntrio.Organizations.Contracts.UseCases.CreateOrganization;

public sealed record CreateOrganizationRequest(string Name);

public sealed record CreateOrganizationResponse(
    Guid OrganizationId,
    string Name,
    string Slug,
    string Status,
    Guid OwnerUserId,
    DateTime CreatedAtUtc);
