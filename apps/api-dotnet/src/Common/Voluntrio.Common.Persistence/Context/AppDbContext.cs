using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Voluntrio.Common.Domain.Abstractions;
using Voluntrio.Common.Infrastructure.Persistence;
using Voluntrio.Common.Persistence.Abstractions;
using Voluntrio.Common.Persistence.Extensions;

namespace Voluntrio.Common.Persistence.Context;

public sealed class AppDbContext : DbContext
{
    private readonly IReadOnlyCollection<IModelBuilderConfiguration> _modelConfigurations;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : this(options, Array.Empty<IModelBuilderConfiguration>())
    {
    }

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IEnumerable<IModelBuilderConfiguration> modelConfigurations)
        : base(options)
    {
        _modelConfigurations = modelConfigurations.ToArray();
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var modelConfiguration in _modelConfigurations)
        {
            modelConfiguration.Configure(modelBuilder);
        }

        modelBuilder.ConfigureOutboxInboxAndIdempotency();

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddDomainEventsToOutbox();
        return await base.SaveChangesAsync(cancellationToken);
    }

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
                Type = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                Payload = JsonSerializer.Serialize(domainEvent)
            }));
        }

        OutboxMessages.AddRange(outboxMessages);
    }
}
