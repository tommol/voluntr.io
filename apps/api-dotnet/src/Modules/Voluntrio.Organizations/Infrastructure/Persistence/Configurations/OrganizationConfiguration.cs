using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voluntrio.Organizations.Domain.Organizations;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    private const string OrganizationSchema = "org";

    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable(
            "organizations",
            OrganizationSchema,
            tableBuilder => tableBuilder.HasCheckConstraint(
                "CK_organizations_parent_not_self",
                "parent_organization_id IS NULL OR parent_organization_id <> id"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name)
            .HasConversion(x => x.Value, x => OrganizationName.Create(x))
            .HasColumnName("name")
            .HasMaxLength(OrganizationName.MaxLength)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasConversion(x => x.Value, x => OrganizationSlug.Create(x))
            .HasColumnName("slug")
            .HasMaxLength(OrganizationSlug.MaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(x => x.Value, x => new OrganizationStatus(x))
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.OwnerUserId)
            .HasColumnName("owner_user_id")
            .IsRequired();

        builder.Property(x => x.ParentOrganizationId)
            .HasColumnName("parent_organization_id");

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => x.ParentOrganizationId);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.ParentOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Members)
            .WithOne()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
