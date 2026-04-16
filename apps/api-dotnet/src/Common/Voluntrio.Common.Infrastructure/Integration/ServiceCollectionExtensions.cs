using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NATS.Client;
using Voluntrio.Common.Application.Integration;

namespace Voluntrio.Common.Infrastructure.Integration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers NATS-based integration event publishing.
    /// </summary>
    public static IServiceCollection AddNatsIntegrationPublishing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<NatsIntegrationOptions>()
            .Bind(configuration.GetSection(NatsIntegrationOptions.SectionName));

        services.AddSingleton<IConnection>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<NatsIntegrationOptions>>().Value;
            var factory = new ConnectionFactory();
            return factory.CreateConnection(options.Url);
        });

        services.AddSingleton<IIntegrationEventPublisher, NatsIntegrationEventPublisher>();

        return services;
    }
}
