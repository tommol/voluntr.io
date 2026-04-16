using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voluntrio.Organizations.Domain.Memberships;
using Voluntrio.Organizations.Domain.ValueObjects;

namespace Voluntrio.Organizations.Infrastructure.Persistence.Configurations;

public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    private const string OrganizationSchema = "org";

    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("organization_memberships", OrganizationSchema);

        builder.HasKey(x => new { x.OrganizationId, x.UserId });

        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion(x => x.Value, x => new MembershipRole(x))
            .HasColumnName("role")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(x => x.Value, x => new MembershipStatus(x))
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.JoinedAtUtc)
            .HasColumnName("joined_at_utc")
            .IsRequired();

        builder.Property(x => x.LeftAtUtc)
            .HasColumnName("left_at_utc");

        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);
    }
}
