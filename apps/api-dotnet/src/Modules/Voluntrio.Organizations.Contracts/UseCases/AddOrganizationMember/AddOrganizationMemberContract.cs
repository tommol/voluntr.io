using Voluntrio.Organizations.Contracts.Shared;

namespace Voluntrio.Organizations.Contracts.UseCases.AddOrganizationMember;

public sealed record AddOrganizationMemberRequest(Guid UserId, string Role);

public sealed record AddOrganizationMemberResponse(
    Guid OrganizationId,
    OrganizationMemberDto Member);
