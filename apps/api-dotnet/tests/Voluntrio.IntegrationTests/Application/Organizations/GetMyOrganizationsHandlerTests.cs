using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Application.UseCases.GetMyOrganizations;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;
using Voluntrio.Organizations.Infrastructure.Persistence;

namespace Voluntrio.IntegrationTests.Application.Organizations;

public sealed class GetMyOrganizationsHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnOrganizations_WhenUserHasActiveMemberships()
    {
        await using var dbContext = CreateDbContext();
        var ownerId = Guid.CreateVersion7();

        var organization = Organization.Create(
            OrganizationName.Create("Main Org"),
            OrganizationSlug.Create("main-org"),
            ownerId,
            DateTime.UtcNow);

        dbContext.Set<Organization>().Add(organization);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMyOrganizationsHandler(dbContext);
        var result = await handler.Handle(new GetMyOrganizationsQuery(ownerId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Organizations);
        Assert.Equal(organization.Id, result.Value.Organizations[0].OrganizationId);
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenUserHasNoActiveMemberships()
    {
        await using var dbContext = CreateDbContext();
        var ownerId = Guid.CreateVersion7();
        var anotherUserId = Guid.CreateVersion7();

        var organization = Organization.Create(
            OrganizationName.Create("Main Org"),
            OrganizationSlug.Create("main-org"),
            ownerId,
            DateTime.UtcNow);

        dbContext.Set<Organization>().Add(organization);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMyOrganizationsHandler(dbContext);
        var result = await handler.Handle(new GetMyOrganizationsQuery(anotherUserId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("auth.forbidden.no_membership", result.Error.Code);
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
