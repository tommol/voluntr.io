using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.ListOrganizationMembers;

namespace Voluntrio.Organizations.Application.UseCases.ListOrganizationMembers;

/// <summary>
/// Query to list members of an organization.
/// </summary>
/// <param name="OrganizationId">The ID of the organization whose members to list.</param>
/// <param name="IncludeInactive">Whether to include inactive (removed) members in the result.</param>
public sealed record ListOrganizationMembersQuery(Guid OrganizationId, bool IncludeInactive)
    : IQuery<Result<ListOrganizationMembersResponse>>;
