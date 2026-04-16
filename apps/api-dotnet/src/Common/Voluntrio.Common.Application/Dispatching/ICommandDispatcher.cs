using Voluntrio.Common.Application.Messaging;

namespace Voluntrio.Common.Application.Dispatching;

public interface ICommandDispatcher
{
    Task<TResponse> Send<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>;
}
