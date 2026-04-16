using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Abstractions;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.CreateOrganization;

namespace Voluntrio.Organizations.Application.UseCases.CreateOrganization;

public sealed class CreateOrganizationEndpoint : IOrganizationsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapPost("/", Handle)
            .WithName("CreateOrganization")
            .WithSummary("Creates a new organization with the current user as owner.");
    }

    private static async Task<IResult> Handle(
        CreateOrganizationRequest request,
        HttpContext httpContext,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var actorResult = EndpointUserContext.ReadActorUserId(httpContext);
        if (actorResult.IsFailure)
        {
            return ResultHttpMapper.ToHttpResult(actorResult.Error);
        }

        var result = await commandDispatcher.Send<CreateOrganizationCommand, Result<CreateOrganizationResponse>>(
            new CreateOrganizationCommand(request.Name, actorResult.Value, EndpointUserContext.ReadIdempotencyKey(httpContext)),
            cancellationToken);

        if (result.IsFailure)
        {
            return ResultHttpMapper.ToHttpResult(result.Error);
        }

        var response = result.Value;

        return Results.Created($"/api/organizations/{response.OrganizationId}", response);
    }
}
