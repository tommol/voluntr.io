using System.Text.Json;
using Voluntrio.Common.Application.Integration;

namespace Voluntrio.Common.Persistence.Integration;

public sealed class SystemTextJsonIntegrationEventSerializer : IIntegrationEventSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize(IIntegrationEvent integrationEvent)
    {
        return JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions);
    }

    public string Serialize<TEvent>(TEvent integrationEvent)
        where TEvent : IIntegrationEvent
    {
        return JsonSerializer.Serialize(integrationEvent, SerializerOptions);
    }

    public object Deserialize(string payload, Type eventType)
    {
        var deserialized = JsonSerializer.Deserialize(payload, eventType, SerializerOptions);
        if (deserialized is null)
        {
            throw new InvalidOperationException($"Unable to deserialize payload into '{eventType.Name}'.");
        }

        return deserialized;
    }
}
