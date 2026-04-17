using Microsoft.EntityFrameworkCore;
using Voluntrio.Common.Persistence.Abstractions;

namespace Voluntrio.Identity.Infrastructure.Persistence;

public sealed class IdentityModelBuilderConfiguration : IModelBuilderConfiguration
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityUser>(builder =>
        {
            builder.ToTable("users", "identity");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(x => x.KindeUserId)
                .HasColumnName("kinde_user_id")
                .HasMaxLength(255);

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(320);

            builder.Property(x => x.GivenName)
                .HasColumnName("given_name")
                .HasMaxLength(200);

            builder.Property(x => x.FamilyName)
                .HasColumnName("family_name")
                .HasMaxLength(200);

            builder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .IsRequired();

            builder.HasIndex(x => x.Email);
            builder.HasIndex(x => x.KindeUserId)
                .IsUnique();
        });

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

            builder.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(x => x.InternalUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
