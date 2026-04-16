namespace Voluntrio.Common.Application.Integration;

/// <summary>
/// Consumes incoming integration envelopes and routes them to handlers.
/// </summary>
public interface IIntegrationEventConsumer
{
    Task ConsumeAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default);
}
