using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Application.Federation;
using Voluntrio.Organizations.Application.UseCases.AddOrganizationMember;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;
using Voluntrio.Organizations.Infrastructure.Persistence;

namespace Voluntrio.IntegrationTests.Application.Organizations;

public sealed class AddOrganizationMemberHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSucceed_WhenUserIsInSameFederation()
    {
        await using var dbContext = CreateDbContext();
        var ownerId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var root = Organization.Create(
            OrganizationName.Create("Root Org"),
            OrganizationSlug.Create("root-org"),
            ownerId,
            DateTime.UtcNow);
        var childA = Organization.Create(
            OrganizationName.Create("Child A"),
            OrganizationSlug.Create("child-a"),
            ownerId,
            DateTime.UtcNow,
            root.Id);
        var childB = Organization.Create(
            OrganizationName.Create("Child B"),
            OrganizationSlug.Create("child-b"),
            ownerId,
            DateTime.UtcNow,
            root.Id);

        childA.AddMember(ownerId, userId, MembershipRole.Member, DateTime.UtcNow);

        dbContext.Set<Organization>().AddRange(root, childA, childB);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(dbContext);
        var result = await handler.Handle(
            new AddOrganizationMemberCommand(childB.Id, ownerId, userId, MembershipRole.Member.Value, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(childB.Id, result.Value.OrganizationId);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserIsInDifferentFederation()
    {
        await using var dbContext = CreateDbContext();
        var ownerA = Guid.CreateVersion7();
        var ownerB = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var rootA = Organization.Create(
            OrganizationName.Create("Root A"),
            OrganizationSlug.Create("root-a"),
            ownerA,
            DateTime.UtcNow);
        var childA = Organization.Create(
            OrganizationName.Create("Child A"),
            OrganizationSlug.Create("child-a-root-a"),
            ownerA,
            DateTime.UtcNow,
            rootA.Id);

        var rootB = Organization.Create(
            OrganizationName.Create("Root B"),
            OrganizationSlug.Create("root-b"),
            ownerB,
            DateTime.UtcNow);
        var childB = Organization.Create(
            OrganizationName.Create("Child B"),
            OrganizationSlug.Create("child-b-root-b"),
            ownerB,
            DateTime.UtcNow,
            rootB.Id);

        childA.AddMember(ownerA, userId, MembershipRole.Member, DateTime.UtcNow);

        dbContext.Set<Organization>().AddRange(rootA, childA, rootB, childB);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(dbContext);
        var result = await handler.Handle(
            new AddOrganizationMemberCommand(childB.Id, ownerB, userId, MembershipRole.Member.Value, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("organizations.members.cross_federation_forbidden", result.Error.Code);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenUserHasNoActiveMemberships()
    {
        await using var dbContext = CreateDbContext();
        var ownerId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var organization = Organization.Create(
            OrganizationName.Create("Standalone Org"),
            OrganizationSlug.Create("standalone-org"),
            ownerId,
            DateTime.UtcNow);

        dbContext.Set<Organization>().Add(organization);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(dbContext);
        var result = await handler.Handle(
            new AddOrganizationMemberCommand(organization.Id, ownerId, userId, MembershipRole.Member.Value, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenOnlyHistoricalMembershipExistsInDifferentFederation()
    {
        await using var dbContext = CreateDbContext();
        var ownerA = Guid.CreateVersion7();
        var ownerB = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();

        var federationA = Organization.Create(
            OrganizationName.Create("Federation A"),
            OrganizationSlug.Create("federation-a"),
            ownerA,
            DateTime.UtcNow);
        var federationB = Organization.Create(
            OrganizationName.Create("Federation B"),
            OrganizationSlug.Create("federation-b"),
            ownerB,
            DateTime.UtcNow);

        federationA.AddMember(ownerA, userId, MembershipRole.Member, DateTime.UtcNow);
        federationA.RemoveMember(ownerA, userId, DateTime.UtcNow.AddMinutes(1));

        dbContext.Set<Organization>().AddRange(federationA, federationB);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(dbContext);
        var result = await handler.Handle(
            new AddOrganizationMemberCommand(federationB.Id, ownerB, userId, MembershipRole.Member.Value, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(federationB.Id, result.Value.OrganizationId);
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

    private static AddOrganizationMemberHandler CreateHandler(AppDbContext dbContext)
    {
        return new AddOrganizationMemberHandler(
            dbContext,
            new FederationMembershipService(dbContext),
            new FixedClock());
    }

    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    }
}
