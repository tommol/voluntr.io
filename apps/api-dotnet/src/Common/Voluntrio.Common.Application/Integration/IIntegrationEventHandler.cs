namespace Voluntrio.Common.Application.Integration;

/// <summary>
/// Handles a typed integration event.
/// </summary>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task Handle(TEvent integrationEvent, CancellationToken cancellationToken);
}
