namespace Voluntrio.Common.Domain.Abstractions;

/// <summary>
/// Base class for domain entities identified by a unique identifier.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets or sets the entity's unique identifier.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The entity's unique identifier.</param>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity by comparing their identifiers.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><c>true</c> if the objects represent the same entity; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Generates a hash code based on the entity's identifier.
    /// </summary>
    /// <returns>A hash code for the entity.</returns>
    public override int GetHashCode() => Id.GetHashCode();
}
