namespace Voluntrio.Common.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
