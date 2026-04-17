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
        var utcNow = DateTime.UtcNow;
        var mapping = await dbContext.Set<ExternalIdentity>()
            .SingleOrDefaultAsync(
                x => x.Provider == provider && x.ExternalSubject == externalSubject,
                cancellationToken);

        if (mapping is null)
        {
            var internalUserId = Guid.CreateVersion7();
            dbContext.Set<IdentityUser>().Add(
                IdentityUser.Create(
                    internalUserId,
                    externalSubject,
                    profileSnapshot.Email,
                    profileSnapshot.GivenName,
                    profileSnapshot.FamilyName,
                    utcNow));

            dbContext.Set<ExternalIdentity>().Add(
                ExternalIdentity.Create(
                    provider,
                    externalSubject,
                    internalUserId,
                    profileSnapshot.Email,
                    profileSnapshot.GivenName,
                    profileSnapshot.FamilyName,
                    utcNow));

            await dbContext.SaveChangesAsync(cancellationToken);
            return internalUserId;
        }

        var user = await dbContext.Set<IdentityUser>()
            .SingleOrDefaultAsync(x => x.Id == mapping.InternalUserId, cancellationToken);

        if (user is null)
        {
            // Backfill user records for existing mappings created before introducing identity users.
            dbContext.Set<IdentityUser>().Add(
                IdentityUser.Create(
                    mapping.InternalUserId,
                    mapping.ExternalSubject,
                    profileSnapshot.Email ?? mapping.Email,
                    profileSnapshot.GivenName ?? mapping.GivenName,
                    profileSnapshot.FamilyName ?? mapping.FamilyName,
                    utcNow));
        }
        else
        {
            if (!string.Equals(user.KindeUserId, mapping.ExternalSubject, StringComparison.Ordinal))
            {
                user.LinkKindeUser(mapping.ExternalSubject, utcNow);
            }

            user.UpdateProfile(
                profileSnapshot.Email,
                profileSnapshot.GivenName,
                profileSnapshot.FamilyName,
                utcNow);
        }

        mapping.UpdateProfile(
            profileSnapshot.Email,
            profileSnapshot.GivenName,
            profileSnapshot.FamilyName,
            utcNow);

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapping.InternalUserId;
    }
}
