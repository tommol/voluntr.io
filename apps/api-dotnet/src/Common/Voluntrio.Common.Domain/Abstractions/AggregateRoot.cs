namespace Voluntrio.Common.Domain.Abstractions;

/// <summary>
/// Base class for domain aggregate roots that can raise and track domain events.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The aggregate root identifier.</param>
    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Gets the collection of domain events raised by the aggregate root.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event that will be tracked by the aggregate root.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears all domain events from the aggregate root.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
