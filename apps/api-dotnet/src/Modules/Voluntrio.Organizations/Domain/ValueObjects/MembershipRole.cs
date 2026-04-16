using Voluntrio.Common.Domain.Exceptions;

namespace Voluntrio.Organizations.Domain.ValueObjects;

public readonly record struct MembershipRole
{
    public static readonly MembershipRole Owner = new("owner");
    public static readonly MembershipRole Admin = new("admin");
    public static readonly MembershipRole Member = new("member");

    public string Value { get; }

    public MembershipRole(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

        if (normalized is not ("owner" or "admin" or "member"))
        {
            throw new DomainException($"Unsupported membership role '{value}'.");
        }

        Value = normalized;
    }

    public override string ToString() => Value;
}
