using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.ListSubOrganizations;
using Voluntrio.Organizations.Domain.Organizations;

namespace Voluntrio.Organizations.Application.UseCases.ListSubOrganizations;

/// <summary>
/// Handles listing direct sub-organizations for a parent organization.
/// </summary>
public sealed class ListSubOrganizationsHandler(AppDbContext dbContext)
    : IQueryHandler<ListSubOrganizationsQuery, Result<ListSubOrganizationsResponse>>
{
    public async Task<Result<ListSubOrganizationsResponse>> Handle(
        ListSubOrganizationsQuery query,
        CancellationToken cancellationToken)
    {
        var parentExists = await dbContext.Set<Organization>()
            .AnyAsync(x => x.Id == query.ParentOrganizationId, cancellationToken);

        if (!parentExists)
        {
            return Result<ListSubOrganizationsResponse>.Failure(
                new Error("organizations.not_found", "Organization was not found."));
        }

        var subOrganizations = await dbContext.Set<Organization>()
            .Where(x => x.ParentOrganizationId == query.ParentOrganizationId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return Result<ListSubOrganizationsResponse>.Success(
            new ListSubOrganizationsResponse(
                query.ParentOrganizationId,
                subOrganizations.Select(x => x.ToSubOrganizationDto()).ToList()));
    }
}
