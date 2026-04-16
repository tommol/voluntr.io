using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Infrastructure.Persistence;

namespace Voluntrio.Organizations.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for <see cref="AppDbContext"/> used by EF Core tooling (migrations) for the Organizations module.
/// </summary>
internal sealed class OrganizationsDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__OrganizationsDb")
            ?? "Host=localhost;Port=5432;Database=voluntrio;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("Voluntrio.Organizations");
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "common");
            })
            .Options;

        return new AppDbContext(
            options,
            [new OrganizationsModelBuilderConfiguration()]);
    }
}
