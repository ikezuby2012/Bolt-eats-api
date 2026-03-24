using Domain.MenuItem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.MenuItemConfig;

internal sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("tbl_menu_item", t =>
        {
            t.HasCheckConstraint("CK_menu_item_price_positive", "price > 0");
            t.HasCheckConstraint("CK_menu_item_discount_price_positive", "discount_price > 0 OR discount_price IS NULL");
            t.HasCheckConstraint("CK_menu_item_discount_less_than_price", "discount_price < price OR discount_price IS NULL");
        });

        builder.HasKey(m => m.Id);

        builder.Property(m => m.RestaurantId)
            .IsRequired()
            .HasColumnName("restaurant_id");

        builder.Property(m => m.CategoryId)
            .IsRequired()
            .HasColumnName("category_id");

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("name");

        builder.Property(m => m.Description)
            .HasMaxLength(1000)
            .HasColumnName("description");

        builder.Property(m => m.Price)
            .HasColumnType("numeric(10,2)")
            .IsRequired()
            .HasColumnName("price");

        builder.Property(m => m.DiscountPrice)
            .HasColumnType("numeric(10,2)")
            .HasColumnName("discount_price");

        builder.Property(m => m.ImageUrl)
            .HasMaxLength(500)
            .HasColumnName("image_url");

        builder.Property(m => m.Calories)
            .HasColumnName("calories");

        builder.Property(m => m.PrepTimeMin)
            .HasColumnName("prep_time_min").HasDefaultValue(15);

        builder.Property(m => m.IsAvailable)
            .IsRequired()
            .HasDefaultValueSql("true")
            .HasColumnName("is_available");

        builder.Property(m => m.IsPopular)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_popular");

        builder.Property(m => m.SortOrder)
            .IsRequired()
            .HasDefaultValue(1)
            .HasColumnName("sort_order");

        builder.Property(m => m.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");

        builder.Property(m => m.CreatedBy)
            .HasMaxLength(100)
            .HasColumnName("created_by");

        builder.Property(m => m.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");

        builder.Property(m => m.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnName("updated_by");

        builder.Property(m => m.IsSoftDeleted)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_soft_deleted");

        builder.HasOne(m => m.Restaurant)
            .WithMany()
            .HasForeignKey(m => m.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasConstraintName("fk_menu_item_restaurant");

        builder.HasOne(m => m.Category)
            .WithMany() // Or .WithMany(c => c.MenuItems) if defined
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Restrict) // Don't delete items if category is removed
            .IsRequired()
            .HasConstraintName("fk_menu_item_category");

        builder.HasIndex(m => new { m.RestaurantId, m.CategoryId })
            .HasDatabaseName("ix_tbl_menu_item_restaurant_category");

        builder.HasQueryFilter(m => !m.IsSoftDeleted);
    }
}
