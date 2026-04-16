using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Organizations.Contracts.UseCases.GetOrganization;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Organizations.Application.UseCases.GetOrganization;

/// <summary>
/// HTTP endpoint for retrieving organization details.
/// </summary>
public sealed class GetOrganizationEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the get organization endpoint to GET /{organizationId}.
    /// </summary>
    /// <param name="organizationsGroup">The route group builder for organizations.</param>
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapGet("/{organizationId:guid}", Handle)
            .WithName("GetOrganization")
            .WithSummary("Gets organization details.");
    }

    private static async Task<IResult> Handle(
        Guid organizationId,
        IQueryDispatcher queryDispatcher,
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

        var result = await queryDispatcher.Send<GetOrganizationQuery, Result<GetOrganizationResponse>>(
            new GetOrganizationQuery(organizationId),
            cancellationToken);
        return result.IsFailure
            ? result.Error.ToHttpResult()
            : Results.Ok(result.Value);
    }
}
