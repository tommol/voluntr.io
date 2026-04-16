namespace Voluntrio.Common.Domain.Abstractions;

/// <summary>
/// Base class for domain value objects that are compared by their atomic values rather than identity.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Gets the collection of atomic values that define this value object's equality.
    /// </summary>
    /// <returns>The atomic values used for equality comparison.</returns>
    protected abstract IEnumerable<object?> GetAtomicValues();

    /// <summary>
    /// Determines whether the specified value object is equal to the current value object.
    /// </summary>
    /// <param name="obj">The value object to compare.</param>
    /// <returns><c>true</c> if the value objects have the same atomic values; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    /// <summary>
    /// Generates a hash code based on all atomic values of this value object.
    /// </summary>
    /// <returns>A hash code for the value object.</returns>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(value => value?.GetHashCode() ?? 0)
            .Aggregate(0, HashCode.Combine);
    }
}
