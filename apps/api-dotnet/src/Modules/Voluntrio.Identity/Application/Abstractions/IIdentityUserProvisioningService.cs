namespace Voluntrio.Identity.Application.Abstractions;

public interface IIdentityUserProvisioningService
{
    Task<Guid> EnsureInternalUserForEmailAsync(
        string email,
        string? givenName,
        string? familyName,
        CancellationToken cancellationToken);
}
