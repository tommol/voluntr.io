using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Abstractions;

namespace Voluntrio.Organizations.Infrastructure.Persistence;

public sealed class OrganizationsModelBuilderConfiguration : IModelBuilderConfiguration
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationsModelBuilderConfiguration).Assembly);
    }
}
