using Voluntrio.Organizations.Contracts.Shared;

namespace Voluntrio.Organizations.Contracts.UseCases.GetOrganization;

public sealed record GetOrganizationResponse(
    Guid OrganizationId,
    string Name,
    string Slug,
    string Status,
    Guid OwnerUserId,
    DateTime CreatedAtUtc,
    IReadOnlyList<OrganizationMemberDto> Members);
