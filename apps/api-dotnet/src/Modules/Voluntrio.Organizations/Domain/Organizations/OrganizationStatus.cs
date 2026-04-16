namespace Voluntrio.Organizations.Domain.Organizations;

public readonly record struct OrganizationStatus(string Value)
{
    public static readonly OrganizationStatus Active = new("active");
}
