using System.Text;
using System.Text.RegularExpressions;
using Voluntrio.Common.Domain.Exceptions;

namespace Voluntrio.Organizations.Domain.ValueObjects;

/// <summary>
/// Represents a URL-friendly slug for an organization.
/// </summary>
public sealed partial record OrganizationSlug
{
    /// <summary>
    /// The maximum length of an organization slug.
    /// </summary>
    public const int MaxLength = 120;

    /// <summary>
    /// Gets the organization slug value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationSlug"/> class.
    /// </summary>
    /// <param name="value">The organization slug value.</param>
    private OrganizationSlug(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new organization slug with validation.
    /// </summary>
    /// <param name="value">The slug value to validate and create.</param>
    /// <returns>A valid organization slug.</returns>
    /// <exception cref="DomainException">Thrown when the slug is empty, exceeds maximum length, or has invalid format.</exception>
    public static OrganizationSlug Create(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainException("Organization slug cannot be empty.");
        }

        if (normalized.Length > MaxLength)
        {
            throw new DomainException($"Organization slug cannot exceed {MaxLength} characters.");
        }

        if (!SlugRegex().IsMatch(normalized))
        {
            throw new DomainException("Organization slug format is invalid.");
        }

        return new OrganizationSlug(normalized);
    }

    /// <summary>
    /// Creates a new organization slug from an organization name.
    /// </summary>
    /// <param name="name">The organization name to convert to a slug.</param>
    /// <returns>A valid organization slug derived from the name.</returns>
    public static OrganizationSlug FromName(OrganizationName name)
    {
        var normalized = RemoveDiacritics(name.Value).ToLowerInvariant();
        var slug = Regex.Replace(normalized, "[^a-z0-9]+", "-").Trim('-');
        slug = Regex.Replace(slug, "-{2,}", "-");

        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "organization";
        }

        if (slug.Length > MaxLength)
        {
            slug = slug[..MaxLength].Trim('-');
        }

        return Create(slug);
    }

    /// <summary>
    /// Removes diacritical marks from text for slug generation.
    /// </summary>
    /// <param name="input">The text to normalize.</param>
    /// <returns>The text with diacritics removed.</returns>
    private static string RemoveDiacritics(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var ch in normalized)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Gets the regex pattern for validating organization slugs.
    /// </summary>
    /// <returns>A compiled regex for slug validation.</returns>
    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();

    public override string ToString() => Value;
}
