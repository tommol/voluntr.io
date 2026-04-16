using Microsoft.Extensions.DependencyInjection;
using Voluntrio.Common.Application.Integration;
using Voluntrio.Common.Persistence.Integration;

namespace Voluntrio.UnitTests.Common.Application.Integration;

public sealed class IntegrationEventConsumerTests
{
    [Fact]
    public async Task ConsumeAsync_ShouldDispatchToRegisteredHandler()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IIntegrationEventSerializer, SystemTextJsonIntegrationEventSerializer>();
        serviceCollection.AddScoped<IIntegrationEventConsumer, IntegrationEventConsumer>();
        serviceCollection.AddScoped<IIntegrationEventHandler<TestIntegrationEvent>, RecordingHandler>();

        using var provider = serviceCollection.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var @event = new TestIntegrationEvent(Guid.CreateVersion7(), DateTime.UtcNow, "alpha");
        var serializer = scope.ServiceProvider.GetRequiredService<IIntegrationEventSerializer>();
        var payload = serializer.Serialize(@event);
        var envelope = new IntegrationEventEnvelope(
            @event.MessageId,
            typeof(TestIntegrationEvent).FullName!,
            payload,
            @event.OccurredOnUtc,
            DateTime.UtcNow);

        var consumer = scope.ServiceProvider.GetRequiredService<IIntegrationEventConsumer>();
        await consumer.ConsumeAsync(envelope, CancellationToken.None);

        var handler = (RecordingHandler)scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<TestIntegrationEvent>>();
        Assert.Single(handler.Handled);
        Assert.Equal("alpha", handler.Handled[0].Value);
    }

    [Fact]
    public async Task ConsumeAsync_ShouldNotThrow_WhenNoHandlersRegistered()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IIntegrationEventSerializer, SystemTextJsonIntegrationEventSerializer>();
        serviceCollection.AddScoped<IIntegrationEventConsumer, IntegrationEventConsumer>();

        using var provider = serviceCollection.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var @event = new TestIntegrationEvent(Guid.CreateVersion7(), DateTime.UtcNow, "beta");
        var serializer = scope.ServiceProvider.GetRequiredService<IIntegrationEventSerializer>();
        var envelope = new IntegrationEventEnvelope(
            @event.MessageId,
            typeof(TestIntegrationEvent).FullName!,
            serializer.Serialize(@event),
            @event.OccurredOnUtc,
            DateTime.UtcNow);

        var consumer = scope.ServiceProvider.GetRequiredService<IIntegrationEventConsumer>();
        await consumer.ConsumeAsync(envelope, CancellationToken.None);
    }

    private sealed record TestIntegrationEvent(Guid MessageId, DateTime OccurredOnUtc, string Value) : IIntegrationEvent;

    private sealed class RecordingHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        public List<TestIntegrationEvent> Handled { get; } = [];

        public Task Handle(TestIntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            Handled.Add(integrationEvent);
            return Task.CompletedTask;
        }
    }
}
