using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Application.Results;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Domain.Memberships;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Application.Shared;

public interface IOrganizationMembershipAuthorization
{
    Task<Result> EnsureActiveMembership(Guid userId, Guid organizationId, CancellationToken cancellationToken);
}

public sealed class OrganizationMembershipAuthorization(AppDbContext dbContext) : IOrganizationMembershipAuthorization
{
    public async Task<Result> EnsureActiveMembership(Guid userId, Guid organizationId, CancellationToken cancellationToken)
    {
        var hasMembership = await dbContext.Set<Membership>()
            .AnyAsync(
                x => x.OrganizationId == organizationId
                     && x.UserId == userId
                     && x.Status == MembershipStatus.Active,
                cancellationToken);

        return hasMembership
            ? Result.Success()
            : Result.Failure(new Error("auth.forbidden", "Active organization membership is required."));
    }
}
