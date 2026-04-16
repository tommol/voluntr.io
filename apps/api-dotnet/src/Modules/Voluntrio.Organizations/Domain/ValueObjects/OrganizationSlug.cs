using System.Text;
using System.Text.RegularExpressions;
using Voluntrio.Common.Domain.Exceptions;

namespace Voluntrio.Organizations.Domain.ValueObjects;

public sealed partial record OrganizationSlug
{
    public const int MaxLength = 120;

    public string Value { get; }

    private OrganizationSlug(string value)
    {
        Value = value;
    }

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

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();

    public override string ToString() => Value;
}
