using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Abstractions;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.RemoveOrganizationMember;

namespace Voluntrio.Organizations.Application.UseCases.RemoveOrganizationMember;

public sealed class RemoveOrganizationMemberEndpoint : IOrganizationsEndpoint
{
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
        CancellationToken cancellationToken)
    {
        var actorResult = EndpointUserContext.ReadActorUserId(httpContext);
        if (actorResult.IsFailure)
        {
            return ResultHttpMapper.ToHttpResult(actorResult.Error);
        }

        var result = await commandDispatcher.Send<RemoveOrganizationMemberCommand, Result<RemoveOrganizationMemberResponse>>(
            new RemoveOrganizationMemberCommand(
                organizationId,
                actorResult.Value,
                userId,
                EndpointUserContext.ReadIdempotencyKey(httpContext)),
            cancellationToken);

        return result.IsFailure
            ? ResultHttpMapper.ToHttpResult(result.Error)
            : Results.Ok(result.Value);
    }
}
