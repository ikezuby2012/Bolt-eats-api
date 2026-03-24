using Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Auth;
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("tbl_refresh_token");

        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id)
            .ValueGeneratedOnAdd();

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("token");

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasColumnName("expires_at");

        builder.Property(rt => rt.IsRevoked)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_revoked");

        builder.Property(rt => rt.IsUsed)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_used");

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(45)  // IPv6 max length (e.g., 2001:0db8:85a3:0000:0000:8a2e:0370:7334)
            .HasColumnName("created_by_ip");

        builder.Property(rt => rt.RevokedByIp)
            .HasMaxLength(45)
            .HasColumnName("revoked_by_ip");

        builder.Property(rt => rt.RevokedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("revoked_at");

        builder.Property(rt => rt.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(rt => rt.CreatedBy)
            .HasMaxLength(100)
            .HasColumnName("created_by");

        builder.Property(rt => rt.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");

        builder.Property(rt => rt.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnName("updated_by");

        builder.Property(rt => rt.IsSoftDeleted)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_soft_deleted");

        builder.HasIndex(rt => rt.CreatedByIp)
            .HasDatabaseName("ix_tbl_refresh_token_created_by_ip")
            .HasFilter("is_soft_deleted = false AND created_by_ip IS NOT NULL");

        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("ix_tbl_refresh_token_token_unique")
            .HasFilter("is_soft_deleted = false");

        // Expiration-based queries (cleanup expired tokens)
        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("ix_tbl_refresh_token_expires_at")
            .HasFilter("is_soft_deleted = false AND is_revoked = false");

        // === Soft Delete Global Filter ===
        builder.HasQueryFilter(rt => !rt.IsSoftDeleted);

    }
}
