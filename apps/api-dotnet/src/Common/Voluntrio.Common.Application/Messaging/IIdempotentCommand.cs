namespace Voluntrio.Common.Application.Messaging;

public interface IIdempotentCommand<out TResponse> : ICommand<TResponse>
{
    string? IdempotencyKey { get; }
}
