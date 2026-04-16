using Voluntrio.Common.Domain.Abstractions;
using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Organizations.Domain.Events;
using Voluntrio.Organizations.Domain.Memberships;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Domain.Organizations;

/// <summary>
/// Represents an organization aggregate root that manages its members and properties.
/// </summary>
public sealed class Organization : AggregateRoot<Guid>
{
    private readonly List<Membership> _members = [];

    /// <summary>
    /// Gets the organization's name.
    /// </summary>
    public OrganizationName Name { get; private set; } = null!;

    /// <summary>
    /// Gets the organization's URL-friendly slug.
    /// </summary>
    public OrganizationSlug Slug { get; private set; } = null!;

    /// <summary>
    /// Gets the current status of the organization.
    /// </summary>
    public OrganizationStatus Status { get; private set; }

    /// <summary>
    /// Gets the user ID of the organization's owner.
    /// </summary>
    public Guid OwnerUserId { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the organization was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the organization was last updated, or null if never updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Gets a read-only collection of the organization's members.
    /// </summary>
    public IReadOnlyCollection<Membership> Members => _members.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Organization"/> class.
    /// </summary>
    private Organization()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Organization"/> class with specified details.
    /// </summary>
    /// <param name="id">The organization's unique identifier.</param>
    /// <param name="name">The organization's name.</param>
    /// <param name="slug">The organization's URL-friendly slug.</param>
    /// <param name="ownerUserId">The user ID of the organization owner.</param>
    /// <param name="createdAtUtc">The UTC timestamp when the organization was created.</param>
    /// <exception cref="DomainException">Thrown when the owner user ID is empty.</exception>
    private Organization(
        Guid id,
        OrganizationName name,
        OrganizationSlug slug,
        Guid ownerUserId,
        DateTime createdAtUtc)
        : base(id)
    {
        if (ownerUserId == Guid.Empty)
        {
            throw new DomainException("Owner user id cannot be empty.");
        }

        Name = name;
        Slug = slug;
        Status = OrganizationStatus.Active;
        OwnerUserId = ownerUserId;
        CreatedAtUtc = createdAtUtc;

        _members.Add(Membership.Create(id, ownerUserId, MembershipRole.Owner, createdAtUtc));

        Raise(new OrganizationCreatedDomainEvent(id, ownerUserId, createdAtUtc));
    }

    /// <summary>
    /// Creates a new organization.
    /// </summary>
    /// <param name="name">The organization's name.</param>
    /// <param name="slug">The organization's URL-friendly slug.</param>
    /// <param name="ownerUserId">The user ID of the organization owner.</param>
    /// <param name="createdAtUtc">The UTC timestamp when the organization is created.</param>
    /// <returns>A new organization instance.</returns>
    public static Organization Create(
        OrganizationName name,
        OrganizationSlug slug,
        Guid ownerUserId,
        DateTime createdAtUtc)
    {
        return new Organization(Guid.CreateVersion7(), name, slug, ownerUserId, createdAtUtc);
    }

    /// <summary>
    /// Adds a new member to the organization.
    /// </summary>
    /// <param name="actorUserId">The user ID performing the action (must have permission).</param>
    /// <param name="userId">The user ID to add as a member.</param>
    /// <param name="role">The membership role to assign.</param>
    /// <param name="utcNow">The current UTC timestamp.</param>
    /// <exception cref="DomainException">Thrown when the actor cannot manage members or the user is already an active member.</exception>
    public void AddMember(Guid actorUserId, Guid userId, MembershipRole role, DateTime utcNow)
    {
        EnsureCanManageMembers(actorUserId);

        var existing = _members.FirstOrDefault(x => x.UserId == userId);
        if (existing is null)
        {
            _members.Add(Membership.Create(Id, userId, role, utcNow));
            Raise(new MemberAddedDomainEvent(Id, userId, utcNow));
            UpdatedAtUtc = utcNow;
            return;
        }

        if (existing.IsActive)
        {
            throw new DomainException("User is already an active member of this organization.");
        }

        existing.Reactivate(role, utcNow);
        Raise(new MemberAddedDomainEvent(Id, userId, utcNow));
        UpdatedAtUtc = utcNow;
    }

    public void ChangeMemberRole(Guid actorUserId, Guid memberUserId, MembershipRole role, DateTime utcNow)
    {
        EnsureCanManageMembers(actorUserId);

        var membership = FindActiveMember(memberUserId);
        if (membership.Role == MembershipRole.Owner)
        {
            throw new DomainException("Owner role cannot be changed.");
        }

        membership.ChangeRole(role);

        Raise(new MemberRoleChangedDomainEvent(Id, memberUserId, role.Value, utcNow));
        UpdatedAtUtc = utcNow;
    }

    public void RemoveMember(Guid actorUserId, Guid memberUserId, DateTime utcNow)
    {
        EnsureCanManageMembers(actorUserId);

        if (memberUserId == OwnerUserId)
        {
            throw new DomainException("Organization owner cannot be removed.");
        }

        var membership = FindActiveMember(memberUserId);
        membership.Remove(utcNow);

        Raise(new MemberRemovedDomainEvent(Id, memberUserId, utcNow));
        UpdatedAtUtc = utcNow;
    }

    private Membership FindActiveMember(Guid userId)
    {
        var membership = _members.FirstOrDefault(x => x.UserId == userId && x.IsActive);

        if (membership is null)
        {
            throw new DomainException("Member does not exist in organization.");
        }

        return membership;
    }

    private void EnsureCanManageMembers(Guid actorUserId)
    {
        var actorMembership = _members.FirstOrDefault(x => x.UserId == actorUserId && x.IsActive);
        if (actorMembership is null || !actorMembership.CanManageMembers())
        {
            throw new DomainException("Only owner or admin can manage organization members.");
        }
    }
}
