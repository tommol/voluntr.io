namespace Voluntrio.Organizations.Contracts.UseCases.RemoveOrganizationMember;

public sealed record RemoveOrganizationMemberResponse(
    Guid OrganizationId,
    Guid UserId,
    string Status,
    DateTime LeftAtUtc);
