namespace Voluntrio.Common.Application.Abstractions;

public interface IIdempotencyStore
{
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken);
    Task MarkProcessedAsync(string key, CancellationToken cancellationToken);
}
