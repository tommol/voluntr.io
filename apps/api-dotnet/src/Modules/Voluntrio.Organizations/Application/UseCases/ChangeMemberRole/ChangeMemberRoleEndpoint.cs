using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Organizations.Contracts.UseCases.ChangeMemberRole;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Organizations.Application.UseCases.ChangeMemberRole;

/// <summary>
/// HTTP endpoint for changing a member's role within an organization.
/// </summary>
public sealed class ChangeMemberRoleEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the change member role endpoint to PUT /{organizationId}/members/{memberUserId}/role.
    /// </summary>
    /// <param name="organizationsGroup">The route group builder for organizations.</param>
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapPut("/{organizationId:guid}/members/{memberUserId:guid}/role", Handle)
            .WithName("ChangeMemberRole")
            .WithSummary("Changes organization member role.");
    }

    private static async Task<IResult> Handle(
        Guid organizationId,
        Guid memberUserId,
        ChangeMemberRoleRequest request,
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

        var result = await commandDispatcher.Send<ChangeMemberRoleCommand, Result<ChangeMemberRoleResponse>>(
            new ChangeMemberRoleCommand(
                organizationId,
                currentUser.InternalUserId.Value,
                memberUserId,
                request.Role,
                EndpointUserContext.ReadIdempotencyKey(httpContext)),
            cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult()
            : Results.Ok(result.Value);
    }
}
