using Voluntrio.Common.Domain.Exceptions;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.UnitTests.Domain.Organizations;

public sealed class OrganizationTests
{
    [Fact]
    public void Create_ShouldCreateOwnerMembership()
    {
        var ownerId = Guid.CreateVersion7();
        var organization = Organization.Create(
            OrganizationName.Create("Voluntr Team"),
            OrganizationSlug.Create("voluntr-team"),
            ownerId,
            DateTime.UtcNow);

        var ownerMembership = organization.Members.Single(x => x.UserId == ownerId);

        Assert.Equal("owner", ownerMembership.Role.Value);
        Assert.True(ownerMembership.IsActive);
    }

    [Fact]
    public void AddMember_ShouldThrow_WhenActorIsNotAdminOrOwner()
    {
        var ownerId = Guid.CreateVersion7();
        var notPrivilegedUser = Guid.CreateVersion7();

        var organization = Organization.Create(
            OrganizationName.Create("Voluntr Team"),
            OrganizationSlug.Create("voluntr-team"),
            ownerId,
            DateTime.UtcNow);

        organization.AddMember(ownerId, notPrivilegedUser, MembershipRole.Member, DateTime.UtcNow);

        var exception = Assert.Throws<DomainException>(() =>
            organization.AddMember(notPrivilegedUser, Guid.CreateVersion7(), MembershipRole.Member, DateTime.UtcNow));

        Assert.Contains("Only owner or admin", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RemoveMember_ShouldThrow_WhenRemovingOwner()
    {
        var ownerId = Guid.CreateVersion7();

        var organization = Organization.Create(
            OrganizationName.Create("Voluntr Team"),
            OrganizationSlug.Create("voluntr-team"),
            ownerId,
            DateTime.UtcNow);

        var exception = Assert.Throws<DomainException>(() =>
            organization.RemoveMember(ownerId, ownerId, DateTime.UtcNow));

        Assert.Contains("owner cannot be removed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_ShouldSetParentOrganizationId_WhenProvided()
    {
        var ownerId = Guid.CreateVersion7();
        var parentOrganizationId = Guid.CreateVersion7();

        var organization = Organization.Create(
            OrganizationName.Create("Voluntr Team Child"),
            OrganizationSlug.Create("team-child"),
            ownerId,
            DateTime.UtcNow,
            parentOrganizationId);

        Assert.Equal(parentOrganizationId, organization.ParentOrganizationId);
    }

    [Fact]
    public void CanManageMembers_ShouldReturnFalse_WhenUserIsNotOwnerOrAdmin()
    {
        var ownerId = Guid.CreateVersion7();
        var memberId = Guid.CreateVersion7();
        var outsiderId = Guid.CreateVersion7();

        var organization = Organization.Create(
            OrganizationName.Create("Voluntr Team"),
            OrganizationSlug.Create("voluntr-team"),
            ownerId,
            DateTime.UtcNow);

        organization.AddMember(ownerId, memberId, MembershipRole.Member, DateTime.UtcNow);

        Assert.False(organization.CanManageMembers(memberId));
        Assert.False(organization.CanManageMembers(outsiderId));
        Assert.True(organization.CanManageMembers(ownerId));
    }
}
