using Microsoft.AspNetCore.Routing;

namespace Voluntrio.Common.Application.Endpoints;

/// <summary>
/// Defines an endpoint that can map itself to a route builder.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder routeBuilder);
}
