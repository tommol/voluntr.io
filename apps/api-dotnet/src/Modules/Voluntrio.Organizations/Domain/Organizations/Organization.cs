using Voluntrio.Common.Domain.Abstractions;
using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Organizations.Domain.Events;
using Voluntrio.Organizations.Domain.Memberships;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Domain.Organizations;

public sealed class Organization : AggregateRoot<Guid>
{
    private readonly List<Membership> _members = [];

    public OrganizationName Name { get; private set; } = null!;
    public OrganizationSlug Slug { get; private set; } = null!;
    public OrganizationStatus Status { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<Membership> Members => _members.AsReadOnly();

    private Organization()
    {
    }

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

    public static Organization Create(
        OrganizationName name,
        OrganizationSlug slug,
        Guid ownerUserId,
        DateTime createdAtUtc)
    {
        return new Organization(Guid.CreateVersion7(), name, slug, ownerUserId, createdAtUtc);
    }

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
