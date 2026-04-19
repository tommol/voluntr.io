using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using Voluntrio.Common.Persistence.Context;
using Voluntrio.Identity.Application.Abstractions;
using Voluntrio.Identity.Infrastructure.Persistence;

namespace Voluntrio.Identity;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityModuleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/identity")
            .WithTags("Identity")
            .RequireAuthorization();

        group.MapGet("/me", async (
                ICurrentUser currentUser,
                AppDbContext dbContext,
                CancellationToken cancellationToken) =>
            {
                if (!currentUser.IsAuthenticated || !currentUser.InternalUserId.HasValue)
                {
                    return Results.Unauthorized();
                }

                var user = await dbContext.Set<IdentityUser>()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == currentUser.InternalUserId.Value, cancellationToken);

                if (user is null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(new
                {
                    user.Id,
                    user.KindeUserId,
                    user.Email,
                    user.GivenName,
                    user.FamilyName,
                    user.AvatarUrl
                });
            })
            .WithName("GetMyIdentityProfile")
            .WithSummary("Returns current user profile from Voluntrio identity store.");

        group.MapPost("/sync", async (
                ClaimsPrincipal principal,
                IIdentityMappingService identityMappingService,
                AppDbContext dbContext,
                CancellationToken cancellationToken) =>
            {
                var externalSubject = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(externalSubject))
                {
                    return Results.Unauthorized();
                }

                var internalUserId = await identityMappingService.ResolveInternalUserIdAsync(
                    "kinde",
                    externalSubject,
                    new IdentityProfileSnapshot(
                        principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email"),
                        principal.FindFirstValue(ClaimTypes.GivenName) ?? principal.FindFirstValue("given_name"),
                        principal.FindFirstValue(ClaimTypes.Surname) ?? principal.FindFirstValue("family_name"),
                        principal.FindFirstValue("picture")),
                    overwriteExistingValues: true,
                    cancellationToken);

                var user = await dbContext.Set<IdentityUser>()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == internalUserId, cancellationToken);

                if (user is null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(new
                {
                    user.Id,
                    user.KindeUserId,
                    user.Email,
                    user.GivenName,
                    user.FamilyName,
                    user.AvatarUrl
                });
            })
            .WithName("SyncMyIdentityProfile")
            .WithSummary("Synchronizes current user profile from identity provider to Voluntrio identity store.");

        return app;
    }
}
