using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Identity.Infrastructure.Persistence;

public sealed class IdentityMappingService(
    AppDbContext dbContext,
    IIdentityUserProvisioningService identityUserProvisioningService) : IIdentityMappingService
{
    public async Task<Guid> ResolveInternalUserIdAsync(
        string provider,
        string externalSubject,
        IdentityProfileSnapshot profileSnapshot,
        bool overwriteExistingValues,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var externalProfile = await identityUserProvisioningService.GetProfileByExternalSubjectAsync(
            provider,
            externalSubject,
            cancellationToken);
        var resolvedEmail = Pick(externalProfile?.Email, profileSnapshot.Email);
        var resolvedGivenName = Pick(externalProfile?.GivenName, profileSnapshot.GivenName);
        var resolvedFamilyName = Pick(externalProfile?.FamilyName, profileSnapshot.FamilyName);
        var resolvedPictureUrl = Pick(externalProfile?.PictureUrl, profileSnapshot.PictureUrl);
        var avatarUrl = IdentityAvatarUrlResolver.Resolve(resolvedEmail, resolvedPictureUrl);
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
                    resolvedEmail,
                    resolvedGivenName,
                    resolvedFamilyName,
                    avatarUrl,
                    utcNow));

            dbContext.Set<ExternalIdentity>().Add(
                ExternalIdentity.Create(
                    provider,
                    externalSubject,
                    internalUserId,
                    resolvedEmail,
                    resolvedGivenName,
                    resolvedFamilyName,
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
                    Pick(resolvedEmail, mapping.Email),
                    Pick(resolvedGivenName, mapping.GivenName),
                    Pick(resolvedFamilyName, mapping.FamilyName),
                    avatarUrl ?? IdentityAvatarUrlResolver.Resolve(Pick(resolvedEmail, mapping.Email), null),
                    utcNow));
        }
        else
        {
            var effectiveEmail = overwriteExistingValues
                ? Pick(resolvedEmail, user.Email, mapping.Email)
                : Pick(user.Email, mapping.Email, resolvedEmail);
            var effectiveGivenName = overwriteExistingValues
                ? Pick(resolvedGivenName, user.GivenName, mapping.GivenName)
                : Pick(user.GivenName, mapping.GivenName, resolvedGivenName);
            var effectiveFamilyName = overwriteExistingValues
                ? Pick(resolvedFamilyName, user.FamilyName, mapping.FamilyName)
                : Pick(user.FamilyName, mapping.FamilyName, resolvedFamilyName);
            var effectiveAvatarUrl = overwriteExistingValues
                ? avatarUrl ?? user.AvatarUrl ?? IdentityAvatarUrlResolver.Resolve(effectiveEmail, null)
                : user.AvatarUrl
                    ?? avatarUrl
                    ?? IdentityAvatarUrlResolver.Resolve(effectiveEmail, null);

            if (!string.Equals(user.KindeUserId, mapping.ExternalSubject, StringComparison.Ordinal))
            {
                user.LinkKindeUser(mapping.ExternalSubject, utcNow);
            }

            user.UpdateProfile(
                effectiveEmail,
                effectiveGivenName,
                effectiveFamilyName,
                effectiveAvatarUrl,
                utcNow);
        }

        var mappingEmail = overwriteExistingValues
            ? Pick(resolvedEmail, mapping.Email)
            : Pick(mapping.Email, resolvedEmail);
        var mappingGivenName = overwriteExistingValues
            ? Pick(resolvedGivenName, mapping.GivenName)
            : Pick(mapping.GivenName, resolvedGivenName);
        var mappingFamilyName = overwriteExistingValues
            ? Pick(resolvedFamilyName, mapping.FamilyName)
            : Pick(mapping.FamilyName, resolvedFamilyName);

        mapping.UpdateProfile(
            mappingEmail,
            mappingGivenName,
            mappingFamilyName,
            utcNow);

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapping.InternalUserId;
    }

    private static string? Pick(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
