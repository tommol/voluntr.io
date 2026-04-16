using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voluntrio.Common.Application.Integration;
using Voluntrio.Common.Infrastructure.Abstractions;
using Voluntrio.Common.Infrastructure.Persistence;

namespace Voluntrio.Common.Infrastructure.Outbox;

/// <summary>
/// Reads integration events from outbox and publishes them through external transport.
/// </summary>
public sealed class IntegrationOutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<IntegrationOutboxProcessor> logger,
    IOptions<OutboxProcessingOptions> options) : BackgroundService
{
    private readonly OutboxProcessingOptions optionsValue = options.Value;

    private static readonly MethodInfo PublishMethod =
        typeof(IIntegrationEventPublisher)
            .GetMethods()
            .Single(m => m is { Name: nameof(IIntegrationEventPublisher.PublishAsync), IsGenericMethodDefinition: true });

    private static readonly Dictionary<string, Type?> TypeCache = new(StringComparer.Ordinal);
    private static readonly Lock TypeCacheLock = new();

    private const int BatchSize = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var batchScope = scopeFactory.CreateScope();
            var outboxStore = batchScope.ServiceProvider.GetRequiredService<IOutboxStore>();

            var batch = await outboxStore.FetchUnprocessedBatchAsync(
                kind: "integration",
                typePrefix: string.Empty,
                batchSize: BatchSize,
                cancellationToken: stoppingToken);

            if (batch.Count == 0)
            {
                await Task.Delay(optionsValue.PollInterval, stoppingToken);
                continue;
            }

            foreach (var message in batch)
            {
                await ProcessMessageAsync(message, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var serializer = serviceProvider.GetRequiredService<IIntegrationEventSerializer>();
            var publisher = serviceProvider.GetRequiredService<IIntegrationEventPublisher>();
            var outboxStore = serviceProvider.GetRequiredService<IOutboxStore>();

            var eventType = ResolveType(message.Type)
                ?? throw new InvalidOperationException($"Integration event type '{message.Type}' was not found.");

            var deserialized = serializer.Deserialize(message.Payload, eventType);
            if (deserialized is not IIntegrationEvent integrationEvent)
            {
                throw new InvalidOperationException($"Payload type '{eventType.FullName}' does not implement IIntegrationEvent.");
            }

            await PublishByRuntimeTypeAsync(publisher, eventType, integrationEvent, cancellationToken);
            await outboxStore.MarkProcessedAsync(message.Id, DateTime.UtcNow, cancellationToken);
        }
        catch (Exception ex)
        {
            using var scope = scopeFactory.CreateScope();
            var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var retryCount = message.RetryCount + 1;

            logger.LogError(
                ex,
                "Outbox integration event processing failed. MessageId={MessageId}, Type={Type}",
                message.Id,
                message.Type);

            if (retryCount >= optionsValue.MaxRetryAttempts)
            {
                await outboxStore.MarkDeadLetterAsync(message.Id, ex.Message, retryCount, DateTime.UtcNow, cancellationToken);
                return;
            }

            await outboxStore.MarkFailedForRetryAsync(
                message.Id,
                ex.Message,
                retryCount,
                DateTime.UtcNow.Add(optionsValue.RetryDelay),
                cancellationToken);
        }
    }

    private async Task PublishByRuntimeTypeAsync(
        IIntegrationEventPublisher publisher,
        Type eventType,
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        var publishGenericMethod = PublishMethod.MakeGenericMethod(eventType);
        var task = publishGenericMethod.Invoke(publisher, [integrationEvent, cancellationToken]) as Task
            ?? throw new InvalidOperationException("Integration event publisher invocation did not return a task.");

        await task;
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
