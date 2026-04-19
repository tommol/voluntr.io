using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Organizations.Contracts.UseCases.RemoveOrganizationMember;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Organizations.Application.UseCases.RemoveOrganizationMember;

/// <summary>
/// HTTP endpoint for removing a member from an organization.
/// </summary>
public sealed class RemoveOrganizationMemberEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the remove organization member endpoint to DELETE /{organizationId}/members/{userId}.
    /// </summary>
    /// <param name="organizationsGroup">The route group builder for organizations.</param>
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapDelete("/{organizationId:guid}/members/{userId:guid}", Handle)
            .WithName("RemoveOrganizationMember")
            .WithSummary("Removes an organization member.");
    }

    private static async Task<IResult> Handle(
        Guid organizationId,
        Guid userId,
        HttpContext httpContext,
        ICommandDispatcher commandDispatcher,
        ICurrentUser currentUser,
        IOrganizationMembershipAuthorization membershipAuthorization,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !currentUser.InternalUserId.HasValue)
        {
            return new Error("auth.unauthorized", "Unauthorized user.").ToHttpResult();
        }

        var membershipResult = await membershipAuthorization.EnsureActiveMembership(
            currentUser.InternalUserId.Value,
            organizationId,
            cancellationToken);

        if (membershipResult.IsFailure)
        {
            return membershipResult.Error.ToHttpResult();
        }

        var result = await commandDispatcher.Send<RemoveOrganizationMemberCommand, Result<RemoveOrganizationMemberResponse>>(
            new RemoveOrganizationMemberCommand(
                organizationId,
                currentUser.InternalUserId.Value,
                userId,
                EndpointUserContext.ReadIdempotencyKey(httpContext)),
            cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult()
            : Results.Ok(result.Value);
    }
}
