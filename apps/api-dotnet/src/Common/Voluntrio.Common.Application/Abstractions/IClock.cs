namespace Voluntrio.Common.Application.Abstractions;

/// <summary>
/// Provides access to current UTC time.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC timestamp.
    /// </summary>
    DateTime UtcNow { get; }
}
