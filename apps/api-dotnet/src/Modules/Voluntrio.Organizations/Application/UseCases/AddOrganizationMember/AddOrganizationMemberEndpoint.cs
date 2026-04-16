using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Abstractions;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.AddOrganizationMember;

namespace Voluntrio.Organizations.Application.UseCases.AddOrganizationMember;

public sealed class AddOrganizationMemberEndpoint : IOrganizationsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapPost("/{organizationId:guid}/members", Handle)
            .WithName("AddOrganizationMember")
            .WithSummary("Adds a member to organization.");
    }

    private static async Task<IResult> Handle(
        Guid organizationId,
        AddOrganizationMemberRequest request,
        HttpContext httpContext,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var actorResult = EndpointUserContext.ReadActorUserId(httpContext);
        if (actorResult.IsFailure)
        {
            return ResultHttpMapper.ToHttpResult(actorResult.Error);
        }

        var result = await commandDispatcher.Send<AddOrganizationMemberCommand, Result<AddOrganizationMemberResponse>>(
            new AddOrganizationMemberCommand(
                organizationId,
                actorResult.Value,
                request.UserId,
                request.Role,
                EndpointUserContext.ReadIdempotencyKey(httpContext)),
            cancellationToken);

        return result.IsFailure
            ? ResultHttpMapper.ToHttpResult(result.Error)
            : Results.Ok(result.Value);
    }
}
