namespace Voluntrio.Common.Application.Messaging;

/// <summary>
/// Represents a command that supports idempotency through an optional idempotency key.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
public interface IIdempotentCommand<out TResponse> : ICommand<TResponse>
{
    /// <summary>
    /// Gets the optional idempotency key used to prevent duplicate command processing.
    /// </summary>
    string? IdempotencyKey { get; }
}
