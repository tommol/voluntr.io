using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Organizations.Domain.Memberships;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Application.Federation;

public sealed class FederationMembershipService(AppDbContext dbContext) : IFederationMembershipService
{
    public async Task<Guid> GetRootOrganizationId(Guid organizationId, CancellationToken cancellationToken)
    {
        var visitedOrganizationIds = new HashSet<Guid>();
        var currentOrganizationId = organizationId;

        while (true)
        {
            if (!visitedOrganizationIds.Add(currentOrganizationId))
            {
                throw new InvalidOperationException("Detected circular organization hierarchy.");
            }

            var parentOrganizationId = await dbContext.Set<Organization>()
                .Where(x => x.Id == currentOrganizationId)
                .Select(x => x.ParentOrganizationId)
                .SingleOrDefaultAsync(cancellationToken);

            if (parentOrganizationId is null)
            {
                if (!await dbContext.Set<Organization>()
                        .AnyAsync(x => x.Id == currentOrganizationId, cancellationToken))
                {
                    throw new InvalidOperationException("Organization was not found.");
                }

                return currentOrganizationId;
            }

            currentOrganizationId = parentOrganizationId.Value;
        }
    }

    public async Task<Guid?> GetUserActiveFederationId(Guid userId, CancellationToken cancellationToken)
    {
        var activeOrganizationIds = await dbContext.Set<Membership>()
            .Where(x => x.UserId == userId && x.Status == MembershipStatus.Active)
            .Select(x => x.OrganizationId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (activeOrganizationIds.Count == 0)
        {
            return null;
        }

        Guid? federationId = null;
        foreach (var activeOrganizationId in activeOrganizationIds)
        {
            var rootOrganizationId = await GetRootOrganizationId(activeOrganizationId, cancellationToken);

            if (federationId is null)
            {
                federationId = rootOrganizationId;
                continue;
            }

            if (federationId.Value != rootOrganizationId)
            {
                throw new InvalidOperationException("User has active memberships in multiple federations.");
            }
        }

        return federationId;
    }
}
