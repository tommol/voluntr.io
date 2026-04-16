using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;
using Voluntrio.Common.Infrastructure.Abstractions;
using Voluntrio.Common.Infrastructure.Persistence;

namespace Voluntrio.Common.Infrastructure.Integration;

/// <summary>
/// Subscribes to NATS integration subjects and persists incoming events into Inbox.
/// </summary>
public sealed class NatsIntegrationInboxSubscriber(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    IOptions<NatsIntegrationOptions> options,
    ILogger<NatsIntegrationInboxSubscriber> logger) : BackgroundService
{
    private readonly NatsIntegrationOptions optionsValue = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subject = BuildWildcardSubject();
        using var subscription = connection.SubscribeSync(subject);

        while (!stoppingToken.IsCancellationRequested)
        {
            Msg? message = null;
            try
            {
                message = subscription.NextMessage(1000);
            }
            catch (NATSTimeoutException)
            {
                // polling timeout, expected when there are no messages
            }

            if (message is null)
            {
                continue;
            }

            await PersistInboxMessageAsync(message, stoppingToken);
        }
    }

    private async Task PersistInboxMessageAsync(Msg message, CancellationToken cancellationToken)
    {
        try
        {
            var eventType = ExtractEventType(message.Subject);
            if (string.IsNullOrWhiteSpace(eventType))
            {
                logger.LogWarning(
                    "NATS inbox message skipped. Processor={Processor}, Subject={Subject}, Reason={Reason}",
                    nameof(NatsIntegrationInboxSubscriber),
                    message.Subject,
                    "UnableToInferEventType");
                return;
            }

            var payload = Encoding.UTF8.GetString(message.Data);
            if (!TryReadMetadata(payload, out var messageId))
            {
                logger.LogWarning(
                    "NATS inbox message skipped. Processor={Processor}, Subject={Subject}, Reason={Reason}",
                    nameof(NatsIntegrationInboxSubscriber),
                    message.Subject,
                    "InvalidPayloadMetadata");
                return;
            }

            using var scope = scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var inboxStore = serviceProvider.GetRequiredService<IInboxStore>();

            if (await inboxStore.ExistsAsync(messageId, cancellationToken))
            {
                return;
            }

            await inboxStore.AddAsync(new InboxMessage
            {
                Id = messageId,
                ReceivedAtUtc = DateTime.UtcNow,
                Type = eventType,
                Payload = payload,
                ProcessedAtUtc = null
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "NATS inbox persistence failed. Processor={Processor}, Subject={Subject}",
                nameof(NatsIntegrationInboxSubscriber),
                message.Subject);
        }
    }

    private string BuildWildcardSubject()
    {
        var prefix = optionsValue.SubjectPrefix.Trim('.');
        return string.IsNullOrWhiteSpace(prefix) ? ">" : $"{prefix}.>";
    }

    private string ExtractEventType(string subject)
    {
        var prefix = optionsValue.SubjectPrefix.Trim('.');
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return subject;
        }

        var prefixWithSeparator = $"{prefix}.";
        return subject.StartsWith(prefixWithSeparator, StringComparison.Ordinal)
            ? subject[prefixWithSeparator.Length..]
            : string.Empty;
    }

    private static bool TryReadMetadata(string payload, out Guid messageId)
    {
        messageId = default;

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            if (!TryGetProperty(root, "messageId", out var messageIdElement) ||
                messageIdElement.ValueKind != JsonValueKind.String ||
                !Guid.TryParse(messageIdElement.GetString(), out messageId))
            {
                return false;
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        var pascalCase = char.ToUpperInvariant(propertyName[0]) + propertyName[1..];
        return element.TryGetProperty(pascalCase, out value);
    }
}
