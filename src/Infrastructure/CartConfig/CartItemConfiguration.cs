using Domain.Cart;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.CartConfig;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("TBL_CART_ITEM", t =>
        {
            t.HasCheckConstraint("CK_cart_item_unit_price_positive", "unit_price > 0");
        });

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CartId)
            .IsRequired()
            .HasColumnName("cart_id");

        builder.Property(c => c.MenuItemId)
            .HasColumnName("menu_item_id");

        builder.Property(c => c.Quantity)
            .IsRequired()
            .HasDefaultValue(1)
            .HasColumnName("Quantity");

        builder.Property(c => c.UnitPrice)
            .IsRequired()
            .HasColumnName("unit_price");

        builder.Property(m => m.Notes)
            .HasMaxLength(2000)
            .HasColumnName("notes");

        builder.Property(c => c.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(100)
            .HasColumnName("created_by");

        builder.Property(c => c.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnName("updated_by");

        builder.HasOne(c => c.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(c => c.CartId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasConstraintName("fk_cart_item_cart");

        builder.HasOne(c => c.MenuItem)
            .WithMany()
            .HasForeignKey(c => c.MenuItemId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired()
            .HasConstraintName("fk_cart_item_menu_item");

        builder.Property(c => c.IsSoftDeleted)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_soft_deleted");

        builder.HasQueryFilter(c => !c.IsSoftDeleted);
    }
}
