using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.GetMyOrganizations;

namespace Voluntrio.Organizations.Application.UseCases.GetMyOrganizations;

public sealed record GetMyOrganizationsQuery(Guid UserId)
    : IQuery<Result<GetMyOrganizationsResponse>>;
