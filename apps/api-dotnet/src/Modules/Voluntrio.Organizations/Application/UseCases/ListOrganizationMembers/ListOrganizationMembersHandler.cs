using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.ListOrganizationMembers;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Common.Persistence.Context;

namespace Voluntrio.Organizations.Application.UseCases.ListOrganizationMembers;

/// <summary>
/// Handles listing members of an organization.
/// </summary>
public sealed class ListOrganizationMembersHandler(AppDbContext dbContext)
    : IQueryHandler<ListOrganizationMembersQuery, Result<ListOrganizationMembersResponse>>
{
    /// <summary>
    /// Handles the list organization members query.
    /// </summary>
    /// <param name="query">The list organization members query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of organization members if found; otherwise, a not found error.</returns>
    public async Task<Result<ListOrganizationMembersResponse>> Handle(ListOrganizationMembersQuery query, CancellationToken cancellationToken)
    {
        var organization = await dbContext.Set<Organization>()
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == query.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<ListOrganizationMembersResponse>.Failure(
                new Error("organizations.not_found", "Organization was not found."));
        }

        var members = query.IncludeInactive
            ? organization.Members
            : organization.Members.Where(x => x.IsActive).ToList();

        var response = new ListOrganizationMembersResponse(
            organization.Id,
            members.Select(x => x.ToDto()).ToList());

        return Result<ListOrganizationMembersResponse>.Success(response);
    }
}
