namespace Voluntrio.Common.Application.Integration;

/// <summary>
/// Marker contract for integration events exchanged between bounded contexts/services.
/// </summary>
public interface IIntegrationEvent
{
    Guid MessageId { get; }
    DateTime OccurredOnUtc { get; }
}
