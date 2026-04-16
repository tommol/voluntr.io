using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Abstractions;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.ListOrganizationMembers;

namespace Voluntrio.Organizations.Application.UseCases.ListOrganizationMembers;

public sealed class ListOrganizationMembersEndpoint : IOrganizationsEndpoint
{
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
        CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.Send<ListOrganizationMembersQuery, Result<ListOrganizationMembersResponse>>(
            new ListOrganizationMembersQuery(organizationId, includeInactive),
            cancellationToken);
        return result.IsFailure
            ? ResultHttpMapper.ToHttpResult(result.Error)
            : Results.Ok(result.Value);
    }
}
