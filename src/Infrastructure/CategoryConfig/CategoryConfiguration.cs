using Domain.Category;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.CategoryConfig;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("tbl_category", t =>
        {
            t.HasCheckConstraint("CK_category_display_order_positive", "display_order > 0");
        });

        builder.HasKey(c => c.Id);

        builder.Property(c => c.RestaurantId)
            .IsRequired()
            .HasColumnName("restaurant_id");

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(1)
            .HasColumnName("display_order");

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValueSql("true")
            .HasColumnName("is_active");

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
            .HasConstraintName("fk_category_restaurant");

        builder.HasQueryFilter(c => !c.IsSoftDeleted);
    }
}
