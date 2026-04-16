using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NATS.Client;
using Voluntrio.Common.Application.Integration;
using System.IO;

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
            var natsOptions = ConnectionFactory.GetDefaultOptions();
            natsOptions.Url = options.Url;

            if (!string.IsNullOrWhiteSpace(options.CredentialsPath))
            {
                if (!File.Exists(options.CredentialsPath))
                {
                    throw new FileNotFoundException("NATS credentials file was not found.", options.CredentialsPath);
                }

                natsOptions.SetUserCredentials(options.CredentialsPath);
            }

            if (options.TlsAllowInsecure)
            {
                natsOptions.TLSRemoteCertificationValidationCallback = (_, _, _, _) => true;
            }

            return factory.CreateConnection(natsOptions);
        });

        services.AddSingleton<IIntegrationEventPublisher, NatsIntegrationEventPublisher>();

        return services;
    }
}
