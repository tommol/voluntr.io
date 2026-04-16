using Voluntrio.Common.Domain.Exceptions;

namespace Voluntrio.Organizations.Domain.ValueObjects;

/// <summary>
/// Represents the status of a membership in an organization.
/// </summary>
public readonly record struct MembershipStatus
{
    /// <summary>
    /// The Active status indicating the member is currently part of the organization.
    /// </summary>
    public static readonly MembershipStatus Active = new("active");

    /// <summary>
    /// The Removed status indicating the member has been removed from the organization.
    /// </summary>
    public static readonly MembershipStatus Removed = new("removed");

    /// <summary>
    /// Gets the status value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MembershipStatus"/> struct.
    /// </summary>
    /// <param name="value">The status value.</param>
    /// <exception cref="DomainException">Thrown when the status value is not recognized.</exception>
    public MembershipStatus(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;

        if (normalized is not ("active" or "removed"))
        {
            throw new DomainException($"Unsupported membership status '{value}'.");
        }

        Value = normalized;
    }

    /// <summary>
    /// Returns the membership status as a string.
    /// </summary>
    /// <returns>The status value.</returns>
    public override string ToString() => Value;
}
