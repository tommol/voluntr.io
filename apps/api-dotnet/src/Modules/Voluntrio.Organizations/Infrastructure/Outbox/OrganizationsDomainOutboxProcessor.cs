using System.Text.Json;
using Microsoft.Extensions.Logging;
using Voluntrio.Common.Application.DomainEvents;
using Voluntrio.Common.Domain.Abstractions;
using Voluntrio.Common.Infrastructure.Abstractions;
using Voluntrio.Common.Infrastructure.Outbox;
using Voluntrio.Common.Infrastructure.Persistence;

namespace Voluntrio.Organizations.Infrastructure.Outbox;

public sealed class OrganizationsDomainOutboxProcessor(
    IOutboxStore outboxStore,
    IDomainEventDispatcher domainEventDispatcher,
    ILogger<OrganizationsDomainOutboxProcessor> logger)
    : ModuleDomainOutboxProcessor(outboxStore, logger)
{
    protected override string DomainEventTypePrefix => "Voluntrio.Organizations.Domain.Events.";

    protected override async Task PublishDomainEventAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var assembly = typeof(OrganizationsDomainOutboxProcessor).Assembly;
        var domainEventType = assembly.GetType(message.Type)
            ?? throw new InvalidOperationException($"Unknown domain event type '{message.Type}'.");

        var domainEvent = JsonSerializer.Deserialize(message.Payload, domainEventType)
            ?? throw new InvalidOperationException($"Unable to deserialize domain event '{message.Type}'.");

        if (domainEvent is not IDomainEvent typedDomainEvent)
        {
            throw new InvalidOperationException($"Type '{message.Type}' is not a domain event.");
        }

        await domainEventDispatcher.DispatchAsync(typedDomainEvent, cancellationToken);
    }
}
