using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Identity.Application.Security;

public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? ExternalSubject => Principal?.FindFirstValue("sub") ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public Guid? InternalUserId
    {
        get
        {
            var raw = Principal?.FindFirstValue(IdentityClaimTypes.InternalUserId);
            return Guid.TryParse(raw, out var internalUserId) ? internalUserId : null;
        }
    }

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
}
