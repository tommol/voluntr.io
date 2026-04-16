namespace Voluntrio.Common.Application.Messaging;

public delegate Task<TResponse> CommandHandlerDelegate<TResponse>();

public interface ICommandPipelineBehavior<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate<TResponse> next);
}
