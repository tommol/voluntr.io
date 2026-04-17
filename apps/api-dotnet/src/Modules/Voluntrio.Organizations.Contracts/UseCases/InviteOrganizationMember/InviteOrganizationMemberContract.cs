using Voluntrio.Organizations.Contracts.Shared;

namespace Voluntrio.Organizations.Contracts.UseCases.InviteOrganizationMember;

public sealed record InviteOrganizationMemberRequest(
    string Email,
    string? GivenName,
    string? FamilyName,
    string Role);

public sealed record InviteOrganizationMemberResponse(
    Guid OrganizationId,
    OrganizationMemberDto Member);
