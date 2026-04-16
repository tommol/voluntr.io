using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Application.UseCases.CreateSubOrganization;
using Voluntrio.Organizations.Application.UseCases.ListSubOrganizations;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;
using Voluntrio.Organizations.Infrastructure.Persistence;

namespace Voluntrio.IntegrationTests.Application.Organizations;

public sealed class SubOrganizationsHandlersTests
{
    [Fact]
    public async Task CreateSubOrganization_ShouldSucceed_WhenActorIsParentOwnerOrAdmin()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var modelConfigurations = new IModelBuilderConfiguration[]
        {
            new OrganizationsModelBuilderConfiguration()
        };

        await using var dbContext = new AppDbContext(options, modelConfigurations);
        var ownerId = Guid.CreateVersion7();
        var adminId = Guid.CreateVersion7();

        var parent = Organization.Create(
            OrganizationName.Create("Parent Org"),
            OrganizationSlug.Create("parent-org"),
            ownerId,
            DateTime.UtcNow);

        parent.AddMember(ownerId, adminId, MembershipRole.Admin, DateTime.UtcNow);

        dbContext.Set<Organization>().Add(parent);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateSubOrganizationHandler(dbContext, new FixedClock());
        var result = await handler.Handle(
            new CreateSubOrganizationCommand(parent.Id, "Child Org", adminId, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(parent.Id, result.Value.ParentOrganizationId);
        Assert.Equal(adminId, result.Value.OwnerUserId);

        var child = await dbContext.Set<Organization>()
            .SingleAsync(x => x.Id == result.Value.OrganizationId, CancellationToken.None);

        Assert.Equal(parent.Id, child.ParentOrganizationId);
    }

    [Fact]
    public async Task CreateSubOrganization_ShouldFail_WhenActorCannotManageParent()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var modelConfigurations = new IModelBuilderConfiguration[]
        {
            new OrganizationsModelBuilderConfiguration()
        };

        await using var dbContext = new AppDbContext(options, modelConfigurations);
        var ownerId = Guid.CreateVersion7();
        var memberId = Guid.CreateVersion7();

        var parent = Organization.Create(
            OrganizationName.Create("Parent Org"),
            OrganizationSlug.Create("parent-org"),
            ownerId,
            DateTime.UtcNow);

        parent.AddMember(ownerId, memberId, MembershipRole.Member, DateTime.UtcNow);

        dbContext.Set<Organization>().Add(parent);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateSubOrganizationHandler(dbContext, new FixedClock());
        var result = await handler.Handle(
            new CreateSubOrganizationCommand(parent.Id, "Child Org", memberId, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("organizations.sub_organizations.create_forbidden", result.Error.Code);
    }

    [Fact]
    public async Task ListSubOrganizations_ShouldReturnDirectChildren()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var modelConfigurations = new IModelBuilderConfiguration[]
        {
            new OrganizationsModelBuilderConfiguration()
        };

        await using var dbContext = new AppDbContext(options, modelConfigurations);
        var ownerId = Guid.CreateVersion7();
        var parent = Organization.Create(
            OrganizationName.Create("Parent Org"),
            OrganizationSlug.Create("parent-org"),
            ownerId,
            DateTime.UtcNow);

        var child1 = Organization.Create(
            OrganizationName.Create("Child One"),
            OrganizationSlug.Create("child-one"),
            ownerId,
            DateTime.UtcNow,
            parent.Id);

        var child2 = Organization.Create(
            OrganizationName.Create("Child Two"),
            OrganizationSlug.Create("child-two"),
            ownerId,
            DateTime.UtcNow,
            parent.Id);

        dbContext.Set<Organization>().AddRange(parent, child1, child2);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new ListSubOrganizationsHandler(dbContext);
        var result = await handler.Handle(new ListSubOrganizationsQuery(parent.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(parent.Id, result.Value.ParentOrganizationId);
        Assert.Equal(2, result.Value.Organizations.Count);
    }

    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    }
}
