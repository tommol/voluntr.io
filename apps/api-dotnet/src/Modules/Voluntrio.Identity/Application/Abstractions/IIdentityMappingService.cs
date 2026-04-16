namespace Voluntrio.Identity.Application.Abstractions;

public interface IIdentityMappingService
{
    Task<Guid> ResolveInternalUserIdAsync(
        string provider,
        string externalSubject,
        IdentityProfileSnapshot profileSnapshot,
        CancellationToken cancellationToken);
}
