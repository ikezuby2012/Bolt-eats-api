using Domain.Cart;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.CartConfig;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("TBL_CART");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(c => c.RestaurantId)
            .IsRequired()
            .HasColumnName("restaurant_id");

        builder.Property(m => m.PromoCode)
            .HasMaxLength(500)
            .HasColumnName("promo_code");

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

        builder.Property(c => c.IsSoftDeleted)
            .IsRequired()
            .HasDefaultValueSql("false")
            .HasColumnName("is_soft_deleted");

        builder.HasOne(c => c.Restaurant)
            .WithMany()
            .HasForeignKey(c => c.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasConstraintName("fk_cart_restaurant");

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasConstraintName("fk_cart_user");

        builder.HasQueryFilter(c => !c.IsSoftDeleted);
    }
}
