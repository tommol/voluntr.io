namespace Voluntrio.Organizations.Contracts.Shared;

public sealed record OrganizationMemberDto(
    Guid UserId,
    string Role,
    string Status,
    DateTime JoinedAtUtc,
    DateTime? LeftAtUtc);
