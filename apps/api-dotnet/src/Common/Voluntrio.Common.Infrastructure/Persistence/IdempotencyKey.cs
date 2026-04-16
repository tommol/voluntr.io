namespace Voluntrio.Common.Infrastructure.Persistence;

public sealed class IdempotencyKey
{
    public string Key { get; init; } = string.Empty;
    public DateTime ProcessedAtUtc { get; init; }
}
