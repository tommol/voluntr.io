using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.GetOrganization;

namespace Voluntrio.Organizations.Application.UseCases.GetOrganization;

/// <summary>
/// Query to retrieve an organization by its identifier.
/// </summary>
/// <param name="OrganizationId">The ID of the organization to retrieve.</param>
public sealed record GetOrganizationQuery(Guid OrganizationId) : IQuery<Result<GetOrganizationResponse>>;
