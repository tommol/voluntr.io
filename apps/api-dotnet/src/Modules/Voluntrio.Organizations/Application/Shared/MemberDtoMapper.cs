using Voluntrio.Organizations.Contracts.Shared;
using Voluntrio.Organizations.Domain.Memberships;

namespace Voluntrio.Organizations.Application.Shared;

internal static class MemberDtoMapper
{
    public static OrganizationMemberDto ToDto(this Membership membership)
    {
        return new OrganizationMemberDto(
            membership.UserId,
            membership.Role.Value,
            membership.Status.Value,
            membership.JoinedAtUtc,
            membership.LeftAtUtc);
    }
}
