using Voluntrio.Common.Domain.Exceptions;

namespace Voluntrio.Organizations.Domain.ValueObjects;

public sealed record OrganizationName
{
    public const int MaxLength = 150;

    public string Value { get; }

    private OrganizationName(string value)
    {
        Value = value;
    }

    public static OrganizationName Create(string value)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainException("Organization name cannot be empty.");
        }

        if (normalized.Length > MaxLength)
        {
            throw new DomainException($"Organization name cannot exceed {MaxLength} characters.");
        }

        return new OrganizationName(normalized);
    }

    public override string ToString() => Value;
}
