using Domain.PromoCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.PromoCodeConfiguration;

internal sealed class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.ToTable("tbl_promo_codes", t =>
        {
            t.HasCheckConstraint("ck_promo_code_discount_type", "lower(discount_type) IN ('fixed', 'percentage')");

            t.HasCheckConstraint("ck_promo_code_discount_value_positive",
               "discount_value > 0");

            t.HasCheckConstraint("ck_promo_code_max_discount_valid",
                "max_discount > 0 OR max_discount IS NULL");

            t.HasCheckConstraint("ck_promo_code_max_discount_not_less_than_value",
                "max_discount >= discount_value OR max_discount IS NULL OR discount_type = 'fixed'");

            t.HasCheckConstraint("ck_promo_code_min_order_value_non_negative",
                "min_order_value >= 0");

            t.HasCheckConstraint("ck_promo_code_usage_limit_positive",
                "usage_limit > 0 OR usage_limit IS NULL");

            t.HasCheckConstraint("ck_promo_code_usage_count_valid",
                "usage_count >= 0 AND (usage_limit IS NULL OR usage_count <= usage_limit)");

            t.HasCheckConstraint("ck_promo_code_expires_at_future",
                "expires_at > NOW() OR expires_at IS NULL");
        });

        builder.HasKey(pc => pc.Id);
        builder.Property(pc => pc.Id)
            .ValueGeneratedOnAdd();

        builder.Property(pc => pc.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("code");

        builder.Property(pc => pc.Description)
            .IsRequired()
            .HasMaxLength(2000)
            .HasColumnName("description");

        // Discount Type: 'fixed' or 'percentage'
        builder.Property(pc => pc.DiscountType)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("fixed")
            .HasColumnName("discount_type");

        builder.Property(pc => pc.DiscountValue)
            .HasColumnType("numeric(10,2)")
            .IsRequired()
            .HasColumnName("discount_value");

        builder.Property(pc => pc.MaxDiscount)
            .HasColumnType("numeric(12,2)")
            .HasColumnName("max_discount");

        builder.Property(pc => pc.MinOrderValue)
            .HasColumnType("numeric(10,2)")
            //.HasDefaultValue(0)
            .HasColumnName("min_order_value");

        builder.Property(pc => pc.MaxDiscountCap).HasColumnType("numeric(18,2)").HasColumnName("max_discount_cap");

        builder.Property(pc => pc.UsageLimit)
            .HasColumnName("usage_limit");

        builder.Property(pc => pc.UsageLimitPerUser).HasColumnName("usage_limit_per_user");

        builder.Property(pc => pc.UsageCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("usage_count");

        builder.Property(pc => pc.StartsAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("starts_at");

        builder.Property(pc => pc.ExpiresAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("expires_at");

        builder.Property(pc => pc.IsActive)
            .IsRequired()
            .HasDefaultValueSql("true")
            .HasColumnName("is_active");

        builder.Property(pc => pc.CreatedAt)
           .HasColumnType("timestamp with time zone")
           .HasColumnName("created_at");

        builder.Property(pc => pc.CreatedBy)
            .HasMaxLength(100)
            .HasColumnName("created_by");

        builder.Property(pc => pc.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");

        builder.Property(pc => pc.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnName("updated_by");

        builder.Property(pc => pc.IsSoftDeleted)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_soft_deleted");

        builder.Property(pc => pc.RestaurantId).HasColumnName("restaurant_id");

        builder.HasOne(a => a.Restaurant)
            .WithMany()
            .HasForeignKey(a => a.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_promo_restaurant");

        builder.HasIndex(pc => pc.Code)
            .IsUnique()
            .HasDatabaseName("x_tbl_promo_code_code_unique")
            .HasFilter("is_soft_deleted = false AND is_active = true");

        builder.HasMany(pc => pc.Usages).WithOne(x => x.PromoCode).HasForeignKey(a => a.PromoCodeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_promo_usages");

        builder.HasQueryFilter(r => !r.IsSoftDeleted);
    }
}
