using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Voluntrio.Common.Application.Endpoints;

public static class EndpointRegistrationExtensions
{
    public static IServiceCollection AddEndpointsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var endpointType = typeof(IEndpoint);

        var endpointImplementations = assembly.GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false } &&
                endpointType.IsAssignableFrom(type));

        foreach (var endpointImplementation in endpointImplementations)
        {
            services.AddSingleton(endpointType, endpointImplementation);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapRegisteredEndpoints(
        this IEndpointRouteBuilder app,
        string routePrefix)
    {
        var group = app.MapGroup(routePrefix);
        group.MapRegisteredEndpoints();
        return app;
    }

    public static IEndpointRouteBuilder MapRegisteredEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var endpoints = routeBuilder.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>();
        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(routeBuilder);
        }

        return routeBuilder;
    }
}
