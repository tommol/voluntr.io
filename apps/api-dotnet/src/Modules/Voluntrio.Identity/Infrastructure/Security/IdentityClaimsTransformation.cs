using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Voluntrio.Identity.Application.Abstractions;
using Voluntrio.Identity.Application.Security;

namespace Voluntrio.Identity.Infrastructure.Security;

public sealed class IdentityClaimsTransformation(IIdentityMappingService identityMappingService) : IClaimsTransformation
{
    private const string ProviderName = "kinde";

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return principal;
        }

        var identity = principal.Identity as ClaimsIdentity;
        if (identity is null)
        {
            return principal;
        }

        var externalSubject = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(externalSubject))
        {
            return principal;
        }

        if (Guid.TryParse(principal.FindFirstValue(IdentityClaimTypes.InternalUserId), out _))
        {
            return principal;
        }

        var internalUserId = await identityMappingService.ResolveInternalUserIdAsync(
            ProviderName,
            externalSubject,
            new IdentityProfileSnapshot(
                principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email"),
                principal.FindFirstValue(ClaimTypes.GivenName) ?? principal.FindFirstValue("given_name"),
                principal.FindFirstValue(ClaimTypes.Surname) ?? principal.FindFirstValue("family_name")),
            CancellationToken.None);

        identity.AddClaim(new Claim(IdentityClaimTypes.InternalUserId, internalUserId.ToString()));

        return principal;
    }
}
