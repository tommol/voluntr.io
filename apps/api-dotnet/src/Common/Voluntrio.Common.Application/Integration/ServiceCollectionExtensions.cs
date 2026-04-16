using Microsoft.Extensions.DependencyInjection;

namespace Voluntrio.Common.Application.Integration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core integration abstractions.
    /// </summary>
    public static IServiceCollection AddIntegrationAbstractions(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, IntegrationEventConsumer>();
        return services;
    }
}
