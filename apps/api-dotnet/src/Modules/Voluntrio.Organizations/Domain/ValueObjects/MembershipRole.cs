using Voluntrio.Common.Domain.Exceptions;

namespace Voluntrio.Organizations.Domain.ValueObjects;

/// <summary>
/// Represents a member's role within an organization.
/// </summary>
public readonly record struct MembershipRole
{
    /// <summary>
    /// The Owner role with full organization management permissions.
    /// </summary>
    public static readonly MembershipRole Owner = new("owner");

    /// <summary>
    /// The Admin role with administrative permissions.
    /// </summary>
    public static readonly MembershipRole Admin = new("admin");

    /// <summary>
    /// The Member role with basic permissions.
    /// </summary>
    public static readonly MembershipRole Member = new("member");

    /// <summary>
    /// Gets the role value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MembershipRole"/> struct.
    /// </summary>
    /// <param name="value">The role value.</param>
    /// <exception cref="DomainException">Thrown when the role value is not recognized.</exception>
    public MembershipRole(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

        if (normalized is not ("owner" or "admin" or "member"))
        {
            throw new DomainException($"Unsupported membership role '{value}'.");
        }

        Value = normalized;
    }

    /// <summary>
    /// Returns the membership role as a string.
    /// </summary>
    /// <returns>The role value.</returns>
    public override string ToString() => Value;
}
