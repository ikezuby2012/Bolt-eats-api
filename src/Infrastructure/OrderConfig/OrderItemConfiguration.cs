using Domain.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.OrderConfig;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("tbl_order_item", schema: "public", t =>
        {
            t.HasCheckConstraint("CK_order_item_quantity_positive", "quantity > 0");
            t.HasCheckConstraint("CK_order_item_unit_price_positive", "unit_price > 0");
            t.HasCheckConstraint("CK_order_item_total_positive",
                "quantity * unit_price > 0");
        });

        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id)
            .ValueGeneratedOnAdd();

        builder.Property(oi => oi.OrderId)
            .IsRequired()
            .HasColumnName("order_id");

        builder.Property(oi => oi.MenuItemId)
            .IsRequired()
            .HasColumnName("menu_item_id");

        builder.Property(oi => oi.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("name");

        builder.Property(oi => oi.Quantity)
            .IsRequired()
            .HasDefaultValue(1)
            .HasColumnName("quantity");

        // === Monetary Fields (Use NUMERIC for precision) ===
        builder.Property(oi => oi.UnitPrice)
            .HasColumnType("numeric(12,2)")
            .IsRequired()
            .HasColumnName("unit_price");

        builder.Property(oi => oi.CreatedAt)
           .HasColumnType("timestamp with time zone")
           .HasColumnName("created_at");

        builder.Property(oi => oi.CreatedBy)
            .HasMaxLength(100)
            .HasColumnName("created_by");

        builder.Property(oi => oi.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("updated_at");

        builder.Property(oi => oi.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnName("updated_by");

        builder.Property(oi => oi.IsSoftDeleted)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_soft_deleted");

        builder.HasOne(oi => oi.Order)
            .WithMany()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade) // Delete items when order is deleted
            .IsRequired()
            .HasConstraintName("fk_order_item_order");

        builder.HasOne(oi => oi.MenuItem)
            .WithMany()
            .HasForeignKey(oi => oi.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired()
            .HasConstraintName("fk_order_item_menu_item");

        builder.HasQueryFilter(r => !r.IsSoftDeleted);
    }
}
