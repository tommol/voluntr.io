using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Identity.Infrastructure.Persistence;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;
using Voluntrio.Organizations.Infrastructure.Persistence;

namespace Voluntrio.UnitTests.Application.Organizations;

public sealed class OrganizationMembershipAuthorizationTests
{
    [Fact]
    public async Task EnsureActiveMembership_ShouldSucceed_ForActiveMember()
    {
        await using var dbContext = CreateDbContext();
        var ownerId = Guid.CreateVersion7();

        var organization = Organization.Create(
            OrganizationName.Create("Access Org"),
            OrganizationSlug.Create("access-org"),
            ownerId,
            DateTime.UtcNow);

        dbContext.Set<Organization>().Add(organization);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var authorization = new OrganizationMembershipAuthorization(dbContext);
        var result = await authorization.EnsureActiveMembership(ownerId, organization.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task EnsureActiveMembership_ShouldFail_ForUserWithoutMembership()
    {
        await using var dbContext = CreateDbContext();
        var ownerId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();

        var organization = Organization.Create(
            OrganizationName.Create("Access Org"),
            OrganizationSlug.Create("access-org"),
            ownerId,
            DateTime.UtcNow);

        dbContext.Set<Organization>().Add(organization);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var authorization = new OrganizationMembershipAuthorization(dbContext);
        var result = await authorization.EnsureActiveMembership(otherUserId, organization.Id, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("auth.forbidden", result.Error.Code);
    }

    [Fact]
    public async Task EnsureActiveMembership_ShouldFail_ForRemovedMembership()
    {
        await using var dbContext = CreateDbContext();
        var ownerId = Guid.CreateVersion7();
        var removedUserId = Guid.CreateVersion7();

        var organization = Organization.Create(
            OrganizationName.Create("Access Org"),
            OrganizationSlug.Create("access-org"),
            ownerId,
            DateTime.UtcNow);

        organization.AddMember(ownerId, removedUserId, MembershipRole.Member, DateTime.UtcNow);
        organization.RemoveMember(ownerId, removedUserId, DateTime.UtcNow.AddMinutes(1));

        dbContext.Set<Organization>().Add(organization);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var authorization = new OrganizationMembershipAuthorization(dbContext);
        var result = await authorization.EnsureActiveMembership(removedUserId, organization.Id, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("auth.forbidden", result.Error.Code);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var modelConfigurations = new IModelBuilderConfiguration[]
        {
            new OrganizationsModelBuilderConfiguration(),
            new IdentityModelBuilderConfiguration()
        };

        return new AppDbContext(options, modelConfigurations);
    }
}
