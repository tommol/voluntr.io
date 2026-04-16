using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Domain.Memberships;

public sealed class Membership
{
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public MembershipRole Role { get; private set; }
    public MembershipStatus Status { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }
    public DateTime? LeftAtUtc { get; private set; }

    private Membership()
    {
    }

    private Membership(Guid organizationId, Guid userId, MembershipRole role, DateTime joinedAtUtc)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainException("Organization id cannot be empty.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("User id cannot be empty.");
        }

        OrganizationId = organizationId;
        UserId = userId;
        Role = role;
        Status = MembershipStatus.Active;
        JoinedAtUtc = joinedAtUtc;
    }

    public static Membership Create(Guid organizationId, Guid userId, MembershipRole role, DateTime joinedAtUtc) =>
        new(organizationId, userId, role, joinedAtUtc);

    public bool IsActive => Status == MembershipStatus.Active;

    public bool CanManageMembers() => Role is { Value: "owner" or "admin" } && IsActive;

    public void ChangeRole(MembershipRole role)
    {
        EnsureActive();
        Role = role;
    }

    public void Remove(DateTime utcNow)
    {
        EnsureActive();
        Status = MembershipStatus.Removed;
        LeftAtUtc = utcNow;
    }

    public void Reactivate(MembershipRole role, DateTime utcNow)
    {
        Role = role;
        Status = MembershipStatus.Active;
        JoinedAtUtc = utcNow;
        LeftAtUtc = null;
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new DomainException("Inactive membership cannot be modified.");
        }
    }
}
