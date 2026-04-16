namespace Voluntrio.Common.Domain.Abstractions;

/// <summary>
/// Exposes domain events captured by an aggregate or entity.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the domain events raised by the entity.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all currently tracked domain events.
    /// </summary>
    void ClearDomainEvents();
}
