using System.Text;
using Microsoft.Extensions.Options;
using NATS.Client;
using Voluntrio.Common.Application.Integration;

namespace Voluntrio.Common.Infrastructure.Integration;

/// <summary>
/// Publishes integration events to NATS subjects.
/// </summary>
public sealed class NatsIntegrationEventPublisher(
    IConnection connection,
    IIntegrationEventSerializer serializer,
    IOptions<NatsIntegrationOptions> options) : IIntegrationEventPublisher
{
    private readonly NatsIntegrationOptions _options = options.Value;

    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        cancellationToken.ThrowIfCancellationRequested();

        var subject = BuildSubject(typeof(TEvent));
        var payload = serializer.Serialize(integrationEvent);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        connection.Publish(subject, payloadBytes);
        return Task.CompletedTask;
    }

    private string BuildSubject(Type eventType)
    {
        var logicalTypeName = (eventType.FullName ?? eventType.Name)
            .Replace('+', '.');

        var prefix = _options.SubjectPrefix.Trim('.');
        return string.IsNullOrWhiteSpace(prefix)
            ? logicalTypeName
            : $"{prefix}.{logicalTypeName}";
    }
}
