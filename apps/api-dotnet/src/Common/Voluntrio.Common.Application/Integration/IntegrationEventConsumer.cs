using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Voluntrio.Common.Application.Integration;

public sealed class IntegrationEventConsumer(
    IServiceProvider serviceProvider,
    IIntegrationEventSerializer serializer) : IIntegrationEventConsumer
{
    private static readonly MethodInfo DispatchMethod =
        typeof(IntegrationEventConsumer)
            .GetMethod(nameof(DispatchInternalAsync), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly Dictionary<string, Type?> TypeCache = new(StringComparer.Ordinal);
    private static readonly Lock TypeCacheLock = new();

    public async Task ConsumeAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var eventType = ResolveType(envelope.EventType)
            ?? throw new InvalidOperationException($"Integration event type '{envelope.EventType}' was not found.");

        var deserialized = serializer.Deserialize(envelope.Payload, eventType);
        if (deserialized is not IIntegrationEvent integrationEvent)
        {
            throw new InvalidOperationException($"Type '{envelope.EventType}' is not an integration event.");
        }

        var dispatch = DispatchMethod.MakeGenericMethod(eventType);
        var task = dispatch.Invoke(this, [integrationEvent, cancellationToken]) as Task
            ?? throw new InvalidOperationException("Integration event dispatch did not return a task.");

        await task;
    }

    private async Task DispatchInternalAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent
    {
        var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.Handle(integrationEvent, cancellationToken);
        }
    }

    private static Type? ResolveType(string fullTypeName)
    {
        lock (TypeCacheLock)
        {
            if (TypeCache.TryGetValue(fullTypeName, out var cached))
            {
                return cached;
            }
        }

        var resolved = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(fullTypeName, throwOnError: false))
            .FirstOrDefault(type => type is not null);

        lock (TypeCacheLock)
        {
            TypeCache[fullTypeName] = resolved;
        }

        return resolved;
    }
}
