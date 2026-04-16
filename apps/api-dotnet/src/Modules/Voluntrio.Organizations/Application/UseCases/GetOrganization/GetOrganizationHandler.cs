using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.GetOrganization;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Common.Persistence.Context;

namespace Voluntrio.Organizations.Application.UseCases.GetOrganization;

/// <summary>
/// Handles retrieval of organization details.
/// </summary>
public sealed class GetOrganizationHandler(AppDbContext dbContext)
    : IQueryHandler<GetOrganizationQuery, Result<GetOrganizationResponse>>
{
    /// <summary>
    /// Handles the get organization query.
    /// </summary>
    /// <param name="query">The get organization query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The organization data if found; otherwise, a not found error.</returns>
    public async Task<Result<GetOrganizationResponse>> Handle(GetOrganizationQuery query, CancellationToken cancellationToken)
    {
        var organization = await dbContext.Set<Organization>()
            .Include(x => x.Members)
            .SingleOrDefaultAsync(x => x.Id == query.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result<GetOrganizationResponse>.Failure(
                new Error("organizations.not_found", "Organization was not found."));
        }

        var response = new GetOrganizationResponse(
            organization.Id,
            organization.Name.Value,
            organization.Slug.Value,
            organization.Status.Value,
            organization.OwnerUserId,
            organization.CreatedAtUtc,
            organization.Members.Select(x => x.ToDto()).ToList());

        return Result<GetOrganizationResponse>.Success(response);
    }
}
