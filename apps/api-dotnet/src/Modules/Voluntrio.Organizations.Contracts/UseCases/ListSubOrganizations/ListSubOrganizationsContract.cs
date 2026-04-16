using Voluntrio.Organizations.Contracts.Shared;

namespace Voluntrio.Organizations.Contracts.UseCases.ListSubOrganizations;

public sealed record ListSubOrganizationsResponse(
    Guid ParentOrganizationId,
    IReadOnlyList<SubOrganizationDto> Organizations);
