namespace Voluntrio.Organizations.Domain.Organizations;

/// <summary>
/// Represents the status of an organization.
/// </summary>
public readonly record struct OrganizationStatus(string Value)
{
    /// <summary>
    /// The Active status indicating the organization is operational.
    /// </summary>
    public static readonly OrganizationStatus Active = new("active");
}
