namespace Voluntrio.Common.Infrastructure.Persistence;

public sealed class InboxMessage
{
    public Guid Id { get; init; }
    public DateTime ReceivedAtUtc { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public DateTime? ProcessedAtUtc { get; set; }
}
