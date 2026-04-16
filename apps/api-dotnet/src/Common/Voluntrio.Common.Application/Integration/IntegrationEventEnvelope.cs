namespace Voluntrio.Common.Application.Integration;

/// <summary>
/// Transport-agnostic envelope for incoming/outgoing integration messages.
/// </summary>
public sealed record IntegrationEventEnvelope(
    Guid MessageId,
    string EventType,
    string Payload,
    DateTime OccurredOnUtc,
    DateTime ReceivedAtUtc);
