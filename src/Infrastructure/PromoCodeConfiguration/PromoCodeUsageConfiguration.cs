using Domain.PromoCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.PromoCodeConfiguration;

internal sealed class PromoCodeUsageConfiguration : IEntityTypeConfiguration<PromoCodeUsage>
{
    public void Configure(EntityTypeBuilder<PromoCodeUsage> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasIndex(u => new { u.PromoCodeId, u.UserId });

        builder.Property(u => u.StatusId).HasDefaultValue(Domain.PromoCode.PromoUsageStatus.Pending.Id).HasColumnName("status_id");
        builder.Property(u => u.UserId).IsRequired().HasColumnName("user_id");
        builder.Property(u => u.PromoCodeId).IsRequired().HasColumnName("promo_code_id");

        builder.HasOne(u => u.PromoCode)
           .WithMany(p => p.Usages)
           .HasForeignKey(u => u.PromoCodeId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(r => !r.IsSoftDeleted);
    }
}
