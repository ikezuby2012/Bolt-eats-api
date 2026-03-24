using Domain.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.RestaurantConfig;

internal sealed class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("tbl_restaurant", t =>
        {
            t.HasCheckConstraint("CK_restaurant_rating_range", "rating BETWEEN 0 AND 5");
            t.HasCheckConstraint("CK_restaurant_total_reviews_non_negative", "total_reviews >= 0");
        });

        builder.HasKey(t => t.Id);

        builder.Property(r => r.OwnerId)
            .IsRequired()
            .HasColumnName("owner_id");

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("name");

        builder.Property(r => r.Description)
            .HasMaxLength(2000)
            .HasColumnName("description");

        builder.Property(r => r.LogoUrl)
            .HasMaxLength(500)
            .HasColumnName("logo_url");

        builder.Property(r => r.BannerUrl)
           .HasMaxLength(500)
           .HasColumnName("banner_url");

        builder.Property(r => r.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("phone_number");

        builder.Property(r => r.Email)
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(r => r.AddressId)
            .HasColumnName("address_id");

        builder.Property(r => r.Rating)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("rating");

        builder.Property(r => r.TotalReviews)
           .IsRequired()
           .HasDefaultValue(0)
           .HasColumnName("total_reviews");

        builder.Property(r => r.DeliveryFeeMin)
            .HasColumnType("numeric(10,2)")
            .HasColumnName("delivery_fee_min");

        builder.Property(r => r.DeliveryFeeMax)
            .HasColumnType("numeric(10,2)")
            .HasColumnName("delivery_fee_max");

        builder.Property(r => r.MinOrderAmount)
            .HasColumnType("numeric(10,2)")
            .HasColumnName("min_order_amount");

        builder.Property(r => r.EstDeliveryMin)
            .HasColumnName("est_delivery_min");

        builder.Property(r => r.EstDeliveryMax)
            .HasColumnName("est_delivery_max");

        builder.Property(r => r.IsOpen)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_open");

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_active");

        builder.Property(r => r.UberOnePartner)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("uber_one_partner");

        builder.Property(r => r.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(100)
            .HasColumnName("created_by");

        builder.Property(r => r.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");

        builder.Property(r => r.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnName("updated_by");

        builder.Property(r => r.IsSoftDeleted)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_soft_deleted");

        builder.HasOne(r => r.Owner)
           .WithMany()
           .HasForeignKey(r => r.OwnerId)
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired()
           .HasConstraintName("fk_restaurant_owner");

        builder.HasOne(r => r.Address)
            .WithMany()
            .HasForeignKey(r => r.AddressId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false)
            .HasConstraintName("fk_restaurant_address");

        builder.HasIndex(r => new { r.IsActive, r.IsOpen })
            .HasDatabaseName("ix_tbl_restaurant_status")
            .HasFilter("is_soft_deleted = false");

        builder.HasIndex(r => r.Rating)
            .HasDatabaseName("ix_tbl_restaurant_rating")
            .HasFilter("is_soft_deleted = false AND is_active = true");

        builder.HasQueryFilter(r => !r.IsSoftDeleted);
    }
}
