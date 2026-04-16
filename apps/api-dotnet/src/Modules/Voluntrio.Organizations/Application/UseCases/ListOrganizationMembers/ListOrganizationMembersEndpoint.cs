using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Organizations.Contracts.UseCases.ListOrganizationMembers;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Organizations.Application.UseCases.ListOrganizationMembers;

/// <summary>
/// HTTP endpoint for listing organization members.
/// </summary>
public sealed class ListOrganizationMembersEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the list organization members endpoint to GET /{organizationId}/members.
    /// </summary>
    /// <param name="organizationsGroup">The route group builder for organizations.</param>
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapGet("/{organizationId:guid}/members", Handle)
            .WithName("ListOrganizationMembers")
            .WithSummary("Lists organization members.");
    }

    private static async Task<IResult> Handle(
        Guid organizationId,
        bool includeInactive,
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

        var result = await queryDispatcher.Send<ListOrganizationMembersQuery, Result<ListOrganizationMembersResponse>>(
            new ListOrganizationMembersQuery(organizationId, includeInactive),
            cancellationToken);
        return result.IsFailure
            ? result.Error.ToHttpResult()
            : Results.Ok(result.Value);
    }
}
