namespace Voluntrio.Common.Domain.Abstractions;

/// <summary>
/// Base class for domain events that capture significant domain model state changes.
/// </summary>
/// <param name="OccurredOnUtc">The UTC timestamp when the domain event occurred.</param>
public abstract record DomainEvent(DateTime OccurredOnUtc) : IDomainEvent;
