using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Organizations.Contracts.UseCases.CreateOrganization;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Organizations.Application.UseCases.CreateOrganization;

/// <summary>
/// HTTP endpoint for creating a new organization.
/// </summary>
public sealed class CreateOrganizationEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the create organization endpoint to POST /.
    /// </summary>
    /// <param name="organizationsGroup">The route group builder for organizations.</param>
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
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !currentUser.InternalUserId.HasValue)
        {
            return new Error("auth.unauthorized", "Unauthorized user.").ToHttpResult();
        }

        var result = await commandDispatcher.Send<CreateOrganizationCommand, Result<CreateOrganizationResponse>>(
            new CreateOrganizationCommand(request.Name, currentUser.InternalUserId.Value, EndpointUserContext.ReadIdempotencyKey(httpContext)),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult();
        }

        var response = result.Value;

        return Results.Created($"/api/organizations/{response.OrganizationId}", response);
    }
}
