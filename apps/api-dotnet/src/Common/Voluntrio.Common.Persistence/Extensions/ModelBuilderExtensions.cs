using Microsoft.EntityFrameworkCore;

namespace Voluntrio.Common.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    private const string CommonSchema = "common";

    public static void ConfigureOutboxInboxAndIdempotency(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Voluntrio.Common.Infrastructure.Persistence.OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_messages", CommonSchema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.OccurredOnUtc).HasColumnName("occurred_on_utc");
            builder.Property(x => x.Kind).HasColumnName("kind").HasMaxLength(30);
            builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(250);
            builder.Property(x => x.Payload).HasColumnName("payload");
            builder.Property(x => x.ProcessedOnUtc).HasColumnName("processed_on_utc");
            builder.Property(x => x.Error).HasColumnName("error");
            builder.Property(x => x.RetryCount).HasColumnName("retry_count");
            builder.Property(x => x.NextRetryOnUtc).HasColumnName("next_retry_on_utc");
            builder.Property(x => x.DeadLetteredOnUtc).HasColumnName("dead_lettered_on_utc");
            builder.HasIndex(x => x.ProcessedOnUtc);
            builder.HasIndex(x => x.NextRetryOnUtc);
            builder.HasIndex(x => x.DeadLetteredOnUtc);
        });

        modelBuilder.Entity<Voluntrio.Common.Infrastructure.Persistence.InboxMessage>(builder =>
        {
            builder.ToTable("inbox_messages", CommonSchema);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.ReceivedAtUtc).HasColumnName("received_at_utc");
            builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(250);
            builder.Property(x => x.Payload).HasColumnName("payload");
            builder.Property(x => x.ProcessedAtUtc).HasColumnName("processed_at_utc");
            builder.Property(x => x.Error).HasColumnName("error");
            builder.Property(x => x.RetryCount).HasColumnName("retry_count");
            builder.Property(x => x.NextRetryOnUtc).HasColumnName("next_retry_on_utc");
            builder.Property(x => x.DeadLetteredOnUtc).HasColumnName("dead_lettered_on_utc");
            builder.HasIndex(x => x.ProcessedAtUtc);
            builder.HasIndex(x => x.NextRetryOnUtc);
            builder.HasIndex(x => x.DeadLetteredOnUtc);
        });

        modelBuilder.Entity<Voluntrio.Common.Infrastructure.Persistence.IdempotencyKey>(builder =>
        {
            builder.ToTable("idempotency_keys", CommonSchema);
            builder.HasKey(x => x.Key);
            builder.Property(x => x.Key).HasColumnName("key").HasMaxLength(200);
            builder.Property(x => x.ProcessedAtUtc).HasColumnName("processed_at_utc");
        });
    }
}
