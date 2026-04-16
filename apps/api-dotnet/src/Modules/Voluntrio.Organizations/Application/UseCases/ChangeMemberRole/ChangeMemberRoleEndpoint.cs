using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Abstractions;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.ChangeMemberRole;

namespace Voluntrio.Organizations.Application.UseCases.ChangeMemberRole;

public sealed class ChangeMemberRoleEndpoint : IOrganizationsEndpoint
{
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
            return ResultHttpMapper.ToHttpResult(actorResult.Error);
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
            ? ResultHttpMapper.ToHttpResult(result.Error)
            : Results.Ok(result.Value);
    }
}
