using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.ListOrganizationMembers;

namespace Voluntrio.Organizations.Application.UseCases.ListOrganizationMembers;

public sealed record ListOrganizationMembersQuery(Guid OrganizationId, bool IncludeInactive)
    : IQuery<Result<ListOrganizationMembersResponse>>;
