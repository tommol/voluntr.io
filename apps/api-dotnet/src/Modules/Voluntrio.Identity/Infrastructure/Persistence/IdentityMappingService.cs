using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Identity.Infrastructure.Persistence;

public sealed class IdentityMappingService(AppDbContext dbContext) : IIdentityMappingService
{
    public async Task<Guid> ResolveInternalUserIdAsync(
        string provider,
        string externalSubject,
        IdentityProfileSnapshot profileSnapshot,
        CancellationToken cancellationToken)
    {
        var mapping = await dbContext.Set<ExternalIdentity>()
            .SingleOrDefaultAsync(
                x => x.Provider == provider && x.ExternalSubject == externalSubject,
                cancellationToken);

        if (mapping is null)
        {
            var internalUserId = Guid.CreateVersion7();
            dbContext.Set<ExternalIdentity>().Add(
                ExternalIdentity.Create(
                    provider,
                    externalSubject,
                    internalUserId,
                    profileSnapshot.Email,
                    profileSnapshot.GivenName,
                    profileSnapshot.FamilyName,
                    DateTime.UtcNow));

            await dbContext.SaveChangesAsync(cancellationToken);
            return internalUserId;
        }

        mapping.UpdateProfile(
            profileSnapshot.Email,
            profileSnapshot.GivenName,
            profileSnapshot.FamilyName,
            DateTime.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapping.InternalUserId;
    }
}
