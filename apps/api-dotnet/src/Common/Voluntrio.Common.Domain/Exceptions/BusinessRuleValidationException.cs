namespace Voluntrio.Common.Domain.Exceptions;

public sealed class BusinessRuleValidationException : DomainException
{
    public BusinessRuleValidationException(string message)
        : base(message)
    {
    }
}
