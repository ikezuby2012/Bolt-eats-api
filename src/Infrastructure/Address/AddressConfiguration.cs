using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Address;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Domain.Address.Address>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Domain.Address.Address> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(a => a.UserId)
            .HasColumnName("user_id");

        builder.Property(a => a.Label)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("label");

        builder.Property(a => a.Street)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("street");

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("city");

        builder.Property(a => a.State)
            .HasMaxLength(100)
            .HasColumnName("state");

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("country");

        builder.Property(a => a.PostalCode)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("postal_code");

        builder.Property(a => a.Latitude)
            .HasColumnType("numeric(9,6)")
            .HasColumnName("latitude");

        builder.Property(a => a.Longitude)
            .HasColumnType("numeric(9,6)")
            .HasColumnName("longitude");

        builder.Property(a => a.LongitudeRaw).HasMaxLength(100).HasColumnName("longitude_raw");
        builder.Property(a => a.LatitudeRaw).HasMaxLength(100).HasColumnName("latitude_raw");


        builder.Property(a => a.IsDefault)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_default");

        builder.Property(a => a.IsSoftDeleted)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_soft_deleted");

        builder.Property(a => a.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(100)
            .HasColumnName("created_by");

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");

        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnName("updated_by");

        builder.HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_address_user");

        builder.HasOne(a => a.Restaurant)
            .WithMany(r => r.Addresses)
            .HasForeignKey(a => a.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_address_restaurant");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_tbl_address_user_id");

        builder.Property(x => x.DeliveryInstructions)
            .HasMaxLength(2000);

        builder.Property(x => x.BuildingType)
            .HasMaxLength(100);

        builder.Property(x => x.AddressLabel)
            .HasMaxLength(100);

        builder.Property(x => x.BuildingDetails)
            .HasColumnType("jsonb")
            .HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)!);

        builder.HasIndex(a => new { a.UserId, a.IsDefault })
            .HasDatabaseName("ix_tbl_address_user_default")
            .IsUnique()
            .HasFilter("is_soft_deleted = false");

        builder.ToTable("TBL_ADDRESS", t =>
        {
            t.HasCheckConstraint("CK_address_latitude_range", "latitude BETWEEN -90 AND 90");
            t.HasCheckConstraint("CK_address_longitude_range", "longitude BETWEEN -180 AND 180");
        });

        builder.HasIndex(a => a.Location).HasMethod("GIST");

        builder.Property(a => a.Location)
               .HasColumnType("geography (point, 4326)");

        builder.HasQueryFilter(a => !a.IsSoftDeleted);
    }
}
