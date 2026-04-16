using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Domain.Memberships;

/// <summary>
/// Represents a user's membership in an organization.
/// </summary>
public sealed class Membership
{
    /// <summary>
    /// Gets the ID of the organization this membership belongs to.
    /// </summary>
    public Guid OrganizationId { get; private set; }

    /// <summary>
    /// Gets the ID of the user who is a member.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the role of the member within the organization.
    /// </summary>
    public MembershipRole Role { get; private set; }

    /// <summary>
    /// Gets the current status of the membership.
    /// </summary>
    public MembershipStatus Status { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the user joined the organization.
    /// </summary>
    public DateTime JoinedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the user left the organization, or null if currently active.
    /// </summary>
    public DateTime? LeftAtUtc { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Membership"/> class.
    /// </summary>
    private Membership()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Membership"/> class with specified details.
    /// </summary>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <param name="userId">The ID of the member user.</param>
    /// <param name="role">The role of the member.</param>
    /// <param name="joinedAtUtc">The UTC timestamp when the user joined.</param>
    /// <exception cref="DomainException">Thrown when organization or user ID is empty.</exception>
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

    /// <summary>
    /// Creates a new membership.
    /// </summary>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <param name="userId">The ID of the user joining.</param>
    /// <param name="role">The role to assign to the member.</param>
    /// <param name="joinedAtUtc">The UTC timestamp when the user joined.</param>
    /// <returns>A new membership instance.</returns>
    public static Membership Create(Guid organizationId, Guid userId, MembershipRole role, DateTime joinedAtUtc) =>
        new(organizationId, userId, role, joinedAtUtc);

    /// <summary>
    /// Gets a value indicating whether the membership is currently active.
    /// </summary>
    public bool IsActive => Status == MembershipStatus.Active;

    /// <summary>
    /// Gets a value indicating whether this member can manage other members.
    /// </summary>
    /// <returns><c>true</c> if the member has owner or admin role and is active; otherwise, <c>false</c>.</returns>
    public bool CanManageMembers() => Role is { Value: "owner" or "admin" } && IsActive;

    /// <summary>
    /// Changes the member's role within the organization.
    /// </summary>
    /// <param name="role">The new role to assign.</param>
    /// <exception cref="DomainException">Thrown when the membership is not active.</exception>
    public void ChangeRole(MembershipRole role)
    {
        EnsureActive();
        Role = role;
    }

    /// <summary>
    /// Removes the member from the organization.
    /// </summary>
    /// <param name="utcNow">The UTC timestamp of the removal.</param>
    /// <exception cref="DomainException">Thrown when the membership is not active.</exception>
    public void Remove(DateTime utcNow)
    {
        EnsureActive();
        Status = MembershipStatus.Removed;
        LeftAtUtc = utcNow;
    }

    /// <summary>
    /// Reactivates a previously removed member with a new role.
    /// </summary>
    /// <param name="role">The new role to assign.</param>
    /// <param name="utcNow">The UTC timestamp of the reactivation.</param>
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
