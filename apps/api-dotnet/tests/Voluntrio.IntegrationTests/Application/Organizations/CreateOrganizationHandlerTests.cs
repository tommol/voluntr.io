using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Abstractions;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Application.UseCases.CreateOrganization;
using Voluntrio.Organizations.Infrastructure.Persistence;

namespace Voluntrio.IntegrationTests.Application.Organizations;

public sealed class CreateOrganizationHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateUniqueSlug_WhenBaseSlugAlreadyExists()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var modelConfigurations = new IModelBuilderConfiguration[]
        {
            new OrganizationsModelBuilderConfiguration()
        };

        await using var dbContext = new AppDbContext(options, modelConfigurations);
        var handler = new CreateOrganizationHandler(dbContext, new FixedClock());

        var owner1 = Guid.CreateVersion7();
        var owner2 = Guid.CreateVersion7();

        var first = await handler.Handle(new CreateOrganizationCommand("Acme Team", owner1, null), CancellationToken.None);
        var second = await handler.Handle(new CreateOrganizationCommand("Acme Team", owner2, null), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal("acme-team", first.Value.Slug);
        Assert.Equal("acme-team-2", second.Value.Slug);
    }

    private sealed class FixedClock : IClock
    {
        public DateTime UtcNow => new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    }
}
