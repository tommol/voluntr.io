using Voluntrio.Organizations.Contracts.Shared;

namespace Voluntrio.Organizations.Contracts.UseCases.ChangeMemberRole;

public sealed record ChangeMemberRoleRequest(string Role);

public sealed record ChangeMemberRoleResponse(
    Guid OrganizationId,
    OrganizationMemberDto Member);
