using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Organizations.Contracts.UseCases.ChangeMemberRole;

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
        CancellationToken cancellationToken)
    {
        var actorResult = EndpointUserContext.ReadActorUserId(httpContext);
        if (actorResult.IsFailure)
        {
            return actorResult.Error.ToHttpResult();
        }

        var result = await commandDispatcher.Send<ChangeMemberRoleCommand, Result<ChangeMemberRoleResponse>>(
            new ChangeMemberRoleCommand(
                organizationId,
                actorResult.Value,
                memberUserId,
                request.Role,
                EndpointUserContext.ReadIdempotencyKey(httpContext)),
            cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult()
            : Results.Ok(result.Value);
    }
}
