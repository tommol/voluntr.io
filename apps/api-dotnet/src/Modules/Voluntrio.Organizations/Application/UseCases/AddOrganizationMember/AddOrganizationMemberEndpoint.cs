using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Organizations.Contracts.UseCases.AddOrganizationMember;
using Voluntrio.Identity.Application.Abstractions;

namespace Voluntrio.Organizations.Application.UseCases.AddOrganizationMember;

/// <summary>
/// HTTP endpoint for adding a member to an organization.
/// </summary>
public sealed class AddOrganizationMemberEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the add organization member endpoint to POST /{organizationId}/members.
    /// </summary>
    /// <param name="organizationsGroup">The route group builder for organizations.</param>
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

        var result = await commandDispatcher.Send<AddOrganizationMemberCommand, Result<AddOrganizationMemberResponse>>(
            new AddOrganizationMemberCommand(
                organizationId,
                currentUser.InternalUserId.Value,
                request.UserId,
                request.Role,
                EndpointUserContext.ReadIdempotencyKey(httpContext)),
            cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult()
            : Results.Ok(result.Value);
    }
}
