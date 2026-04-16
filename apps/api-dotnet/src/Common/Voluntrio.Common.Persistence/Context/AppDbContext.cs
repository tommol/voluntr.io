using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Voluntrio.Common.Application.Integration;
using Voluntrio.Common.Domain.Abstractions;
using Voluntrio.Common.Infrastructure.Persistence;
using Voluntrio.Common.Persistence.Integration;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Extensions;

namespace Voluntrio.Common.Persistence.Context;

/// <summary>
/// The application's primary database context for Entity Framework Core.
/// </summary>
public sealed class AppDbContext : DbContext
{
    private readonly IReadOnlyCollection<IModelBuilderConfiguration> _modelConfigurations;
    private readonly IReadOnlyCollection<IDomainEventToIntegrationEventMapper> _domainEventMappers;
    private readonly IIntegrationEventSerializer _integrationEventSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : this(
            options,
            Array.Empty<IModelBuilderConfiguration>(),
            Array.Empty<IDomainEventToIntegrationEventMapper>(),
            new SystemTextJsonIntegrationEventSerializer())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class with model builder configurations.
    /// </summary>
    /// <param name="options">The database context options.</param>
    /// <param name="modelConfigurations">The model builder configurations for entity mapping.</param>
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IEnumerable<IModelBuilderConfiguration> modelConfigurations)
        : this(
            options,
            modelConfigurations,
            Array.Empty<IDomainEventToIntegrationEventMapper>(),
            new SystemTextJsonIntegrationEventSerializer())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class with model and integration event mappings.
    /// </summary>
    /// <param name="options">The database context options.</param>
    /// <param name="modelConfigurations">The model builder configurations for entity mapping.</param>
    /// <param name="domainEventMappers">Domain-to-integration event mappers.</param>
    /// <param name="integrationEventSerializer">Integration event serializer.</param>
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IEnumerable<IModelBuilderConfiguration> modelConfigurations,
        IEnumerable<IDomainEventToIntegrationEventMapper> domainEventMappers,
        IIntegrationEventSerializer integrationEventSerializer)
        : base(options)
    {
        _modelConfigurations = modelConfigurations.ToArray();
        _domainEventMappers = domainEventMappers.ToArray();
        _integrationEventSerializer = integrationEventSerializer;
    }

    /// <summary>
    /// Gets or sets the database set of outbox messages.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Gets or sets the database set of inbox messages.
    /// </summary>
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    /// <summary>
    /// Gets or sets the database set of idempotency keys.
    /// </summary>
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    /// <summary>
    /// Configures the database model using the provided model builder configurations.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var modelConfiguration in _modelConfigurations)
        {
            modelConfiguration.Configure(modelBuilder);
        }

        modelBuilder.ConfigureOutboxInboxAndIdempotency();

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Saves all entity changes to the database and publishes domain events to the outbox.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddDomainEventsToOutbox();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Adds all domain events from tracked entities to the outbox for external publication.
    /// </summary>
    private void AddDomainEventsToOutbox()
    {
        var entitiesWithDomainEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Count != 0)
            .Select(entry => entry.Entity)
            .ToList();

        if (entitiesWithDomainEvents.Count == 0)
        {
            return;
        }

        var outboxMessages = new List<OutboxMessage>();
        foreach (var entity in entitiesWithDomainEvents)
        {
            var domainEvents = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();

            outboxMessages.AddRange(domainEvents.Select(domainEvent => new OutboxMessage
            {
                Id = Guid.CreateVersion7(),
                OccurredOnUtc = domainEvent.OccurredOnUtc,
                Kind = "domain",
                Type = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
            }));

            foreach (var domainEvent in domainEvents)
            {
                var integrationEvents = _domainEventMappers
                    .Where(mapper => mapper.CanMap(domainEvent))
                    .SelectMany(mapper => mapper.Map(domainEvent))
                    .ToList();

                outboxMessages.AddRange(integrationEvents.Select(integrationEvent => new OutboxMessage
                {
                    Id = integrationEvent.MessageId,
                    OccurredOnUtc = integrationEvent.OccurredOnUtc,
                    Kind = "integration",
                    Type = integrationEvent.GetType().FullName ?? integrationEvent.GetType().Name,
                    Payload = _integrationEventSerializer.Serialize(integrationEvent)
                }));
            }
        }

        OutboxMessages.AddRange(outboxMessages);
    }
}
