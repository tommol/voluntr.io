namespace Voluntrio.Common.Application.Integration;

/// <summary>
/// Publishes integration events to external transport (or durable outbox).
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
