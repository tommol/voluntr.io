namespace Voluntrio.Common.Application.Abstractions;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
