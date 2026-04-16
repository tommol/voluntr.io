using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Voluntrio.Common.Infrastructure.Abstractions;
using Voluntrio.Common.Infrastructure.Persistence;

namespace Voluntrio.Common.Infrastructure.Outbox;

public abstract class ModuleDomainOutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger logger) : BackgroundService
{
    protected virtual int BatchSize => 100;
    protected virtual TimeSpan PollInterval => TimeSpan.FromSeconds(2);
    protected abstract string DomainEventTypePrefix { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var batchScope = scopeFactory.CreateScope();
            var outboxStore = batchScope.ServiceProvider.GetRequiredService<IOutboxStore>();

            var batch = await outboxStore.FetchUnprocessedBatchAsync(
                kind: "domain",
                typePrefix: DomainEventTypePrefix,
                batchSize: BatchSize,
                cancellationToken: stoppingToken);

            if (batch.Count == 0)
            {
                await Task.Delay(PollInterval, stoppingToken);
                continue;
            }

            foreach (var message in batch)
            {
                await ProcessMessage(message, stoppingToken);
            }
        }
    }

    private async Task ProcessMessage(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var outboxStore = serviceProvider.GetRequiredService<IOutboxStore>();

            await PublishDomainEventAsync(serviceProvider, message, cancellationToken);
            await outboxStore.MarkProcessedAsync(message.Id, DateTime.UtcNow, cancellationToken);
        }
        catch (Exception ex)
        {
            using var scope = scopeFactory.CreateScope();
            var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            logger.LogError(ex, "Outbox domain event processing failed. MessageId={MessageId}, Type={Type}", message.Id, message.Type);
            await outboxStore.MarkFailedAsync(message.Id, ex.Message, cancellationToken);
        }
    }

    protected abstract Task PublishDomainEventAsync(
        IServiceProvider serviceProvider,
        OutboxMessage message,
        CancellationToken cancellationToken);
}
