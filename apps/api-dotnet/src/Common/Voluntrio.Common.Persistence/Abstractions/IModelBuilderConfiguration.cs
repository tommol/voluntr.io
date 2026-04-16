using Microsoft.EntityFrameworkCore;

namespace Voluntrio.Common.Persistence.Abstractions;

public interface IModelBuilderConfiguration
{
    void Configure(ModelBuilder modelBuilder);
}
