using Voluntrio.Common.Application.Dispatching;
using Voluntrio.Common.Application.Results;
using Voluntrio.Organizations.Application.Abstractions;
using Voluntrio.Organizations.Application.Shared;
using Voluntrio.Organizations.Contracts.UseCases.GetOrganization;

namespace Voluntrio.Organizations.Application.UseCases.GetOrganization;

public sealed class GetOrganizationEndpoint : IOrganizationsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder organizationsGroup)
    {
        organizationsGroup.MapGet("/{organizationId:guid}", Handle)
            .WithName("GetOrganization")
            .WithSummary("Gets organization details.");
    }

    private static async Task<IResult> Handle(
        Guid organizationId,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.Send<GetOrganizationQuery, Result<GetOrganizationResponse>>(
            new GetOrganizationQuery(organizationId),
            cancellationToken);
        return result.IsFailure
            ? ResultHttpMapper.ToHttpResult(result.Error)
            : Results.Ok(result.Value);
    }
}
