using Domain.Review;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.ReviewConfig;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("tbl_review", t =>
        {
            t.HasCheckConstraint("CK_review_rating", "rating >= 1 and rating <= 5");
        });

        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id)
            .ValueGeneratedOnAdd();

        builder.Property(oi => oi.OrderId)
            .IsRequired()
            .HasColumnName("order_id");

        builder.Property(o => o.UserId)
           .IsRequired()
           .HasColumnName("user_id");

        builder.Property(m => m.RestaurantId)
            .IsRequired()
            .HasColumnName("restaurant_id");

        builder.Property(m => m.Rating).HasDefaultValue(1).HasColumnName("rating");

        builder.Property(o => o.Comment)
            .HasMaxLength(3000)
            .HasColumnName("comment");

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

        builder.HasOne(oi => oi.Order)
            .WithMany()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasConstraintName("fk_rating_order");

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired()
            .HasConstraintName("fk_rating_user");

        // Order → Restaurant
        builder.HasOne(o => o.Restaurant)
            .WithMany()
            .HasForeignKey(o => o.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired()
            .HasConstraintName("fk_rating_restaurant");

        builder.HasQueryFilter(r => !r.IsSoftDeleted);
    }
}
