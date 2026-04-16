using Voluntrio.Common.Domain.Exceptions;

namespace Voluntrio.Organizations.Domain.ValueObjects;

public readonly record struct MembershipStatus
{
    public static readonly MembershipStatus Active = new("active");
    public static readonly MembershipStatus Removed = new("removed");

    public string Value { get; }

    public MembershipStatus(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

        if (normalized is not ("active" or "removed"))
        {
            throw new DomainException($"Unsupported membership status '{value}'.");
        }

        Value = normalized;
    }

    public override string ToString() => Value;
}
