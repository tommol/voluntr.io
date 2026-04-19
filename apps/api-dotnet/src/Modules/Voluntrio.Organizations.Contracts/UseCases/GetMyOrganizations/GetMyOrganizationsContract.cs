namespace Voluntrio.Organizations.Contracts.UseCases.GetMyOrganizations;

public sealed record MyOrganizationDto(
    Guid OrganizationId,
    string Name,
    string Slug,
    string Role);

public sealed record GetMyOrganizationsResponse(
    IReadOnlyList<MyOrganizationDto> Organizations);
