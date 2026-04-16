using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Abstractions;

namespace Voluntrio.Identity.Infrastructure.Persistence;

public sealed class IdentityModelBuilderConfiguration : IModelBuilderConfiguration
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExternalIdentity>(builder =>
        {
            builder.ToTable("external_identities", "identity");

            builder.HasKey(x => new { x.Provider, x.ExternalSubject });

            builder.Property(x => x.Provider)
                .HasColumnName("provider")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ExternalSubject)
                .HasColumnName("external_subject")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.InternalUserId)
                .HasColumnName("internal_user_id")
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(320);

            builder.Property(x => x.GivenName)
                .HasColumnName("given_name")
                .HasMaxLength(200);

            builder.Property(x => x.FamilyName)
                .HasColumnName("family_name")
                .HasMaxLength(200);

            builder.Property(x => x.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .IsRequired();

            builder.HasIndex(x => x.InternalUserId)
                .IsUnique();
        });
    }
}
