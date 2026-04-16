namespace Voluntrio.Common.Domain.Exceptions;

/// <summary>
/// Represents an exception that occurs when a business rule validation fails.
/// </summary>
public sealed class BusinessRuleValidationException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message describing the failed business rule validation.</param>
    public BusinessRuleValidationException(string message)
        : base(message)
    {
    }
}
