namespace Voluntrio.Common.Application.Integration;

/// <summary>
/// Serializes/deserializes integration payloads.
/// </summary>
public interface IIntegrationEventSerializer
{
    string Serialize(IIntegrationEvent integrationEvent);

    string Serialize<TEvent>(TEvent integrationEvent)
        where TEvent : IIntegrationEvent;

    object Deserialize(string payload, Type eventType);
}
