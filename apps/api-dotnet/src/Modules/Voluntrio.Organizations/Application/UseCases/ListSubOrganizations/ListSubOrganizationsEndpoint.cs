using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Endpoints;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Contracts.UseCases.ListSubOrganizations;

namespace Voluntrio.Organizations.Application.UseCases.ListSubOrganizations;

/// <summary>
/// HTTP endpoint for listing direct sub-organizations.
/// </summary>
public sealed class ListSubOrganizationsEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the list sub-organizations endpoint to GET /{organizationId}/sub-organizations.
    /// </summary>
    /// <param name="organizationsGroup">The route group builder for organizations.</param>
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapGet("/{organizationId:guid}/sub-organizations", Handle)
            .WithName("ListSubOrganizations")
            .WithSummary("Lists direct sub-organizations of an organization.");
    }

    private static async Task<IResult> Handle(
        Guid organizationId,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.Send<ListSubOrganizationsQuery, Result<ListSubOrganizationsResponse>>(
            new ListSubOrganizationsQuery(organizationId),
            cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult()
            : Results.Ok(result.Value);
    }
}
