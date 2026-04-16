namespace Voluntrio.Common.Application.Abstractions;

/// <summary>
/// Provides access to the current system time in UTC.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <summary>
    /// Gets the current UTC timestamp.
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
}
