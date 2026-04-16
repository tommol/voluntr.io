using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.CreateSubOrganization;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Organizations.Application.UseCases.CreateSubOrganization;

/// <summary>
/// HTTP endpoint for creating a new sub-organization.
/// </summary>
public sealed class CreateSubOrganizationEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the create sub-organization endpoint to POST /{parentOrganizationId}/sub-organizations.
    /// </summary>
    /// <param name="organizationsGroup">The route group builder for organizations.</param>
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapPost("/{parentOrganizationId:guid}/sub-organizations", Handle)
            .WithName("CreateSubOrganization")
            .WithSummary("Creates a new sub-organization under the specified organization.");
    }

    private static async Task<IResult> Handle(
        Guid parentOrganizationId,
        CreateSubOrganizationRequest request,
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
            parentOrganizationId,
            cancellationToken);

        if (membershipResult.IsFailure)
        {
            return membershipResult.Error.ToHttpResult();
        }

        var result = await commandDispatcher.Send<CreateSubOrganizationCommand, Result<CreateSubOrganizationResponse>>(
            new CreateSubOrganizationCommand(
                parentOrganizationId,
                request.Name,
                currentUser.InternalUserId.Value,
                EndpointUserContext.ReadIdempotencyKey(httpContext)),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult();
        }

        var response = result.Value;
        return Results.Created($"/api/organizations/{response.OrganizationId}", response);
    }
}
