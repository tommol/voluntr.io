using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Messaging;
using Voluntrio.Common.Application.Results;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Contracts.UseCases.GetMyOrganizations;
using Voluntrio.Organizations.Domain.Memberships;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Application.UseCases.GetMyOrganizations;

public sealed class GetMyOrganizationsHandler(AppDbContext dbContext)
    : IQueryHandler<GetMyOrganizationsQuery, Result<GetMyOrganizationsResponse>>
{
    public async Task<Result<GetMyOrganizationsResponse>> Handle(
        GetMyOrganizationsQuery query,
        CancellationToken cancellationToken)
    {
        var organizations = await (
            from membership in dbContext.Set<Membership>()
            join organization in dbContext.Set<Organization>() on membership.OrganizationId equals organization.Id
            where membership.UserId == query.UserId && membership.Status == MembershipStatus.Active
            orderby membership.JoinedAtUtc
            select new MyOrganizationDto(
                organization.Id,
                organization.Name.Value,
                organization.Slug.Value,
                membership.Role.Value))
            .ToListAsync(cancellationToken);

        if (organizations.Count == 0)
        {
            return Result<GetMyOrganizationsResponse>.Failure(
                new Error("auth.forbidden.no_membership", "User has no active organization memberships."));
        }

        return Result<GetMyOrganizationsResponse>.Success(new GetMyOrganizationsResponse(organizations));
    }
}
