namespace Voluntrio.Common.Domain.Exceptions;

/// <summary>
/// Represents an exception that occurs when a domain business rule is violated.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="message">The error message that describes the domain violation.</param>
    public DomainException(string message)
        : base(message)
    {
    }
}
