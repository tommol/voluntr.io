using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Common.Application.Results;
using Voluntrio.Identity.Application.Abstractions;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.InviteOrganizationMember;

namespace Voluntrio.Organizations.Application.UseCases.InviteOrganizationMember;

public sealed class InviteOrganizationMemberEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapPost("/{organizationId:guid}/members/invitations", Handle)
            .WithName("InviteOrganizationMember")
            .WithSummary("Invites a member to organization and provisions user identity.");
    }

    private static async Task<IResult> Handle(
        Guid organizationId,
        InviteOrganizationMemberRequest request,
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

        var result = await commandDispatcher.Send<InviteOrganizationMemberCommand, Result<InviteOrganizationMemberResponse>>(
            new InviteOrganizationMemberCommand(
                organizationId,
                currentUser.InternalUserId.Value,
                request.Email,
                request.GivenName,
                request.FamilyName,
                request.Role,
                EndpointUserContext.ReadIdempotencyKey(httpContext)),
            cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult()
            : Results.Ok(result.Value);
    }
}
