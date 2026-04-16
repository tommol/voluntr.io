using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Voluntrio.Common.Application.Integration;
using Voluntrio.Common.Infrastructure.Abstractions;

namespace Voluntrio.Common.Infrastructure.Inbox;

public abstract class ModuleIntegrationInboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger logger) : BackgroundService
{
    protected virtual int BatchSize => 100;
    protected virtual TimeSpan PollInterval => TimeSpan.FromSeconds(2);
    protected abstract string IntegrationEventTypePrefix { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var batchScope = scopeFactory.CreateScope();
            var inboxStore = batchScope.ServiceProvider.GetRequiredService<IInboxStore>();

            var batch = await inboxStore.FetchUnprocessedBatchAsync(
                typePrefix: IntegrationEventTypePrefix,
                batchSize: BatchSize,
                cancellationToken: stoppingToken);

            if (batch.Count == 0)
            {
                await Task.Delay(PollInterval, stoppingToken);
                continue;
            }

            foreach (var message in batch)
            {
                await ProcessMessageAsync(message, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(Voluntrio.Common.Infrastructure.Persistence.InboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var inboxStore = serviceProvider.GetRequiredService<IInboxStore>();
            var consumer = serviceProvider.GetRequiredService<IIntegrationEventConsumer>();

            var envelope = new IntegrationEventEnvelope(
                MessageId: message.Id,
                EventType: message.Type,
                Payload: message.Payload,
                OccurredOnUtc: message.ReceivedAtUtc,
                ReceivedAtUtc: message.ReceivedAtUtc);

            await consumer.ConsumeAsync(envelope, cancellationToken);
            await inboxStore.MarkProcessedAsync(message.Id, DateTime.UtcNow, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Inbox integration event processing failed. MessageId={MessageId}, Type={Type}",
                message.Id,
                message.Type);
        }
    }
}
