namespace Voluntrio.Common.Domain.Abstractions;

/// <summary>
/// Represents a domain event raised by the domain model.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    DateTime OccurredOnUtc { get; }
}
