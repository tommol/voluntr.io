using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;
using Voluntrio.Organizations.Infrastructure.Persistence;

namespace Voluntrio.IntegrationTests.Application.Organizations;

public sealed class OutboxIntegrationTests
{
    [Fact]
    public async Task SaveChanges_ShouldPersistOutboxMessages_ForDomainEvents()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var modelConfigurations = new IModelBuilderConfiguration[]
        {
            new OrganizationsModelBuilderConfiguration()
        };

        await using var dbContext = new AppDbContext(options, modelConfigurations);

        var organization = Organization.Create(
            OrganizationName.Create("Outbox Org"),
            OrganizationSlug.Create("outbox-org"),
            Guid.CreateVersion7(),
            DateTime.UtcNow);

        dbContext.Set<Organization>().Add(organization);
        await dbContext.SaveChangesAsync();

        Assert.True(await dbContext.OutboxMessages.AnyAsync());
    }
}
