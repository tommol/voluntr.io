using Voluntrio.Common.Domain.Exceptions;

namespace Voluntrio.Organizations.Domain.ValueObjects;

/// <summary>
/// Represents the name of an organization.
/// </summary>
public sealed record OrganizationName
{
    /// <summary>
    /// The maximum length of an organization name.
    /// </summary>
    public const int MaxLength = 150;

    /// <summary>
    /// Gets the organization name value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationName"/> class.
    /// </summary>
    /// <param name="value">The organization name value.</param>
    private OrganizationName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new organization name with validation.
    /// </summary>
    /// <param name="value">The organization name to validate and create.</param>
    /// <returns>A valid organization name.</returns>
    /// <exception cref="DomainException">Thrown when the name is empty or exceeds maximum length.</exception>
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

    /// <summary>
    /// Returns the organization name as a string.
    /// </summary>
    /// <returns>The organization name value.</returns>
    public override string ToString() => Value;
}
