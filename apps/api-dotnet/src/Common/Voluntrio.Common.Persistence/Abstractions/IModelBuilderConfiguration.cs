using Microsoft.EntityFrameworkCore;

namespace Voluntrio.Common.Persistence.Abstractions;

/// <summary>
/// Defines Entity Framework model configuration for a module.
/// </summary>
public interface IModelBuilderConfiguration
{
    /// <summary>
    /// Applies entity model configuration.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    void Configure(ModelBuilder modelBuilder);
}
