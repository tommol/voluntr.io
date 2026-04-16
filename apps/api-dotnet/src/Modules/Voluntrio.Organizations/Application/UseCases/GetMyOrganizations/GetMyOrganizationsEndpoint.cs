using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Common.Application.Results;
using Voluntrio.Identity.Application.Abstractions;
using Voluntrio.Organizations.Contracts.UseCases.GetMyOrganizations;

namespace Voluntrio.Organizations.Application.UseCases.GetMyOrganizations;

public sealed class GetMyOrganizationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapGet("/me/organizations", Handle)
            .WithName("GetMyOrganizations")
            .WithSummary("Lists active organizations for the current user.");
    }

    private static async Task<IResult> Handle(
        IQueryDispatcher queryDispatcher,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !currentUser.InternalUserId.HasValue)
        {
            return new Error("auth.unauthorized", "Unauthorized user.").ToHttpResult();
        }

        var result = await queryDispatcher.Send<GetMyOrganizationsQuery, Result<GetMyOrganizationsResponse>>(
            new GetMyOrganizationsQuery(currentUser.InternalUserId.Value),
            cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult()
            : Results.Ok(result.Value);
    }
}
