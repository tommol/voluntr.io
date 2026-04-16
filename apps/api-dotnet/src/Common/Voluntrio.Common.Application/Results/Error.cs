namespace Voluntrio.Common.Application.Results;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
/// <param name="Code">The error code identifier.</param>
/// <param name="Message">The error message description.</param>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Gets an empty error representing the absence of an error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}
