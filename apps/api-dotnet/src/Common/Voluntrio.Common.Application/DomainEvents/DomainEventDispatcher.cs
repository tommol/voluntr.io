using Microsoft.Extensions.DependencyInjection;
using Voluntrio.Common.Domain.Abstractions;

namespace Voluntrio.Common.Application.DomainEvents;

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var dispatchMethod = typeof(DomainEventDispatcher)
            .GetMethod(nameof(DispatchInternalAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .MakeGenericMethod(domainEvent.GetType());

        return (Task)dispatchMethod.Invoke(this, [domainEvent, cancellationToken])!;
    }

    private async Task DispatchInternalAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken)
        where TDomainEvent : IDomainEvent
    {
        var handlers = serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>();
        foreach (var handler in handlers)
        {
            await handler.Handle(domainEvent, cancellationToken);
        }
    }
}
