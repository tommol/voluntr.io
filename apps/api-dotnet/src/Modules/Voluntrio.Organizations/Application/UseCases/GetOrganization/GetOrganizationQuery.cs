using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.GetOrganization;

namespace Voluntrio.Organizations.Application.UseCases.GetOrganization;

public sealed record GetOrganizationQuery(Guid OrganizationId) : IQuery<Result<GetOrganizationResponse>>;
