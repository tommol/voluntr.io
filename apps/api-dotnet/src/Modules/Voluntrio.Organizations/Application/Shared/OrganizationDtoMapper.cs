using Voluntrio.Organizations.Contracts.Shared;
using Voluntrio.Organizations.Domain.Organizations;

namespace Voluntrio.Organizations.Application.Shared;

internal static class OrganizationDtoMapper
{
    public static SubOrganizationDto ToSubOrganizationDto(this Organization organization)
    {
        if (!organization.ParentOrganizationId.HasValue)
        {
            throw new InvalidOperationException("Parent organization id is required for sub-organization DTO mapping.");
        }

        return new SubOrganizationDto(
            organization.Id,
            organization.Name.Value,
            organization.Slug.Value,
            organization.Status.Value,
            organization.OwnerUserId,
            organization.ParentOrganizationId.Value,
            organization.CreatedAtUtc);
    }
}
