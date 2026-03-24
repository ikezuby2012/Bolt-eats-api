using Domain.Rider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.RiderConfig;

internal sealed class RiderLocationConfiguration : IEntityTypeConfiguration<RiderLocation>
{
    public void Configure(EntityTypeBuilder<RiderLocation> builder)
    {
        builder.ToTable("tbl_rider_location");

        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id)
            .ValueGeneratedOnAdd();

        builder.Property(oi => oi.OrderId)
            .IsRequired()
            .HasColumnName("order_id");

        builder.Property(oi => oi.RiderId)
            .IsRequired()
            .HasColumnName("rider_id");

        builder.Property(a => a.Latitude)
           .HasColumnType("numeric(9,6)")
           .HasColumnName("latitude");

        builder.Property(a => a.Longitude)
            .HasColumnType("numeric(9,6)")
            .HasColumnName("longitude");

        builder.Property(a => a.LongitudeRaw).HasMaxLength(100).HasColumnName("longitude_raw");
        builder.Property(a => a.LatitudeRaw).HasMaxLength(100).HasColumnName("latitude_raw");

        builder.Property(m => m.Bearing)
            .HasColumnType("numeric(12,2)")
            .HasDefaultValue(0)
            .HasColumnName("bearing");

        builder.Property(m => m.Speed)
            .HasColumnType("numeric(12,2)")
            .HasDefaultValue(0)
            .HasColumnName("speed");

        builder.Property(o => o.RecordedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("recorded_at");

        builder.Property(o => o.CreatedAt)
           .HasColumnType("timestamp with time zone")
           .HasColumnName("created_at");

        builder.Property(o => o.CreatedBy)
            .HasMaxLength(100)
            .HasColumnName("created_by");

        builder.Property(o => o.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");

        builder.Property(o => o.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnName("updated_by");

        builder.Property(o => o.IsSoftDeleted)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_soft_deleted");

        builder.HasOne(o => o.Rider)
            .WithMany()
            .HasForeignKey(o => o.RiderId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false)
            .HasConstraintName("fk_rider_location_rider");

        builder.HasOne(oi => oi.Order)
            .WithMany()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasConstraintName("fk_rider_location_order");

        builder.HasQueryFilter(r => !r.IsSoftDeleted);
    }
}
