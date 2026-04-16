using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.ListSubOrganizations;

namespace Voluntrio.Organizations.Application.UseCases.ListSubOrganizations;

/// <summary>
/// Query to list direct sub-organizations of an organization.
/// </summary>
/// <param name="ParentOrganizationId">The parent organization identifier.</param>
public sealed record ListSubOrganizationsQuery(Guid ParentOrganizationId)
    : IQuery<Result<ListSubOrganizationsResponse>>;
