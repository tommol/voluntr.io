using Voluntrio.Organizations.Contracts.Shared;

namespace Voluntrio.Organizations.Contracts.UseCases.ListOrganizationMembers;

public sealed record ListOrganizationMembersResponse(
    Guid OrganizationId,
    IReadOnlyList<OrganizationMemberDto> Members);
