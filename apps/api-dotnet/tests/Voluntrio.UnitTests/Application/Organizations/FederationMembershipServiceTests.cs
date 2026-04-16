using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Application.Federation;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;
using Voluntrio.Organizations.Infrastructure.Persistence;

namespace Voluntrio.UnitTests.Application.Organizations;

public sealed class FederationMembershipServiceTests
{
    [Fact]
    public async Task GetRootOrganizationId_ShouldReturnTopMostParent_WhenOrganizationHasAncestors()
    {
        await using var dbContext = CreateDbContext();
        var ownerId = Guid.CreateVersion7();

        var root = Organization.Create(
            OrganizationName.Create("Root"),
            OrganizationSlug.Create("root"),
            ownerId,
            DateTime.UtcNow);
        var child = Organization.Create(
            OrganizationName.Create("Child"),
            OrganizationSlug.Create("child"),
            ownerId,
            DateTime.UtcNow,
            root.Id);
        var grandChild = Organization.Create(
            OrganizationName.Create("Grand Child"),
            OrganizationSlug.Create("grand-child"),
            ownerId,
            DateTime.UtcNow,
            child.Id);

        dbContext.Set<Organization>().AddRange(root, child, grandChild);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var service = new FederationMembershipService(dbContext);
        var result = await service.GetRootOrganizationId(grandChild.Id, CancellationToken.None);

        Assert.Equal(root.Id, result);
    }

    [Fact]
    public async Task GetRootOrganizationId_ShouldReturnSameId_WhenOrganizationIsRoot()
    {
        await using var dbContext = CreateDbContext();
        var ownerId = Guid.CreateVersion7();

        var root = Organization.Create(
            OrganizationName.Create("Root"),
            OrganizationSlug.Create("root-single"),
            ownerId,
            DateTime.UtcNow);

        dbContext.Set<Organization>().Add(root);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var service = new FederationMembershipService(dbContext);
        var result = await service.GetRootOrganizationId(root.Id, CancellationToken.None);

        Assert.Equal(root.Id, result);
    }

    [Fact]
    public async Task GetUserActiveFederationId_ShouldIgnoreRemovedMemberships()
    {
        await using var dbContext = CreateDbContext();
        var ownerA = Guid.CreateVersion7();
        var ownerB = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var orgA = Organization.Create(
            OrganizationName.Create("Org A"),
            OrganizationSlug.Create("org-a"),
            ownerA,
            DateTime.UtcNow);
        var orgB = Organization.Create(
            OrganizationName.Create("Org B"),
            OrganizationSlug.Create("org-b"),
            ownerB,
            DateTime.UtcNow);

        orgA.AddMember(ownerA, userId, MembershipRole.Member, DateTime.UtcNow);
        orgA.RemoveMember(ownerA, userId, DateTime.UtcNow.AddMinutes(1));
        orgB.AddMember(ownerB, userId, MembershipRole.Member, DateTime.UtcNow.AddMinutes(2));

        dbContext.Set<Organization>().AddRange(orgA, orgB);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var service = new FederationMembershipService(dbContext);
        var result = await service.GetUserActiveFederationId(userId, CancellationToken.None);

        Assert.Equal(orgB.Id, result);
    }

    [Fact]
    public async Task GetUserActiveFederationId_ShouldReturnNull_WhenNoActiveMemberships()
    {
        await using var dbContext = CreateDbContext();
        var service = new FederationMembershipService(dbContext);

        var result = await service.GetUserActiveFederationId(Guid.CreateVersion7(), CancellationToken.None);

        Assert.Null(result);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var modelConfigurations = new IModelBuilderConfiguration[]
        {
            new OrganizationsModelBuilderConfiguration()
        };

        return new AppDbContext(options, modelConfigurations);
    }
}
