using Domain.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.OrderConfig;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("tbl_order", t =>
        {
            t.HasCheckConstraint("CK_order_subtotal_positive", "subtotal >= 0");
            t.HasCheckConstraint("CK_order_delivery_fee_positive", "delivery_fee >= 0");
            t.HasCheckConstraint("CK_order_discount_positive", "discount >= 0 OR discount IS NULL");
            t.HasCheckConstraint("CK_order_tax_positive", "tax >= 0");
            t.HasCheckConstraint("CK_order_total_positive", "total >= 0");
            t.HasCheckConstraint("CK_order_total_calculation",
                "total = subtotal + delivery_fee + tax - COALESCE(discount, 0)");
            t.HasCheckConstraint("CK_order_discount_not_exceed_subtotal",
                "discount <= subtotal OR discount IS NULL");
            t.HasCheckConstraint("CK_order_checkout_before_accepted",
                "checkout_at <= accepted_at OR accepted_at IS NULL");
            t.HasCheckConstraint("CK_order_accepted_before_pickedup",
                "accepted_at <= picked_up_at OR picked_up_at IS NULL");
            t.HasCheckConstraint("CK_order_pickedup_before_delivered",
                "picked_up_at <= delivered_at OR delivered_at IS NULL");
        });

        builder.HasKey(m => m.Id);

        builder.Property(o => o.CustomerId)
           .IsRequired()
           .HasColumnName("customer_id");

        builder.Property(m => m.RestaurantId)
            .IsRequired()
            .HasColumnName("restaurant_id");

        builder.Property(o => o.RiderId)
            .HasColumnName("rider_id");

        builder.Property(o => o.AddressId)
            .IsRequired()
            .HasColumnName("address_id");

        builder.Property(o => o.SubTotal)
            .HasColumnType("numeric(12,2)")
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("subtotal");

        builder.Property(o => o.DeliveryFee)
            .HasColumnType("numeric(12,2)")
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("delivery_fee");

        builder.Property(o => o.OrderStatusId).HasDefaultValue(1).HasColumnName("order_status_id");

        builder.Property(o => o.Discount)
            .HasColumnType("numeric(12,2)")
            .HasColumnName("discount");

        builder.Property(o => o.Tax)
            .HasColumnType("numeric(12,2)")
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("tax");

        builder.Property(o => o.Total)
            .HasColumnType("numeric(12,2)")
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("total");

        builder.Property(o => o.PromoCode)
            .HasMaxLength(50)
            .HasColumnName("promo_code");

        builder.Property(o => o.PaymentRef)
            .HasMaxLength(100)
            .HasColumnName("payment_ref");

        builder.Property(o => o.Notes)
            .HasMaxLength(2000)
            .HasColumnName("notes");

        builder.Property(o => o.CheckoutAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("checkout_at");

        builder.Property(o => o.AcceptedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("accepted_at");

        builder.Property(o => o.PickedUpAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("picked_up_at");

        builder.Property(o => o.DeliveredAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("delivered_at");

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

        builder.HasOne(o => o.Customer)
            .WithMany() // Or .WithMany(u => u.Orders) if defined
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired()
            .HasConstraintName("fk_order_customer");

        // Order → Restaurant
        builder.HasOne(o => o.Restaurant)
            .WithMany() // Or .WithMany(r => r.Orders) if defined
            .HasForeignKey(o => o.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict) // Preserve order history
            .IsRequired()
            .HasConstraintName("fk_order_restaurant");

        // Order → Rider (User) - Optional
        builder.HasOne(o => o.Rider)
            .WithMany()
            .HasForeignKey(o => o.RiderId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false)
            .HasConstraintName("fk_order_rider");

        builder.HasOne(o => o.Address)
            .WithMany()
            .HasForeignKey(o => o.AddressId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired()
            .HasConstraintName("fk_order_address");

        builder.HasOne(o => o.OrderStatus)
           .WithMany()
           .HasForeignKey(o => o.OrderStatusId)
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired()
           .HasConstraintName("fk_order_status");

        builder.HasQueryFilter(r => !r.IsSoftDeleted);
    }
}
