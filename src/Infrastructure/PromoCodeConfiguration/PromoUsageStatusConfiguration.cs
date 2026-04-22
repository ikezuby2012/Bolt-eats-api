using Domain.PromoCode;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.PromoCodeConfiguration;

internal sealed class PromoUsageStatusConfiguration : IEntityTypeConfiguration<PromoUsageStatus>
{
    public void Configure(EntityTypeBuilder<PromoUsageStatus> builder)
    {
        builder.ToTable("TBL_PROMO_USAGE_STATUS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(500);

        builder.HasData(PromoUsageStatus.GetValues());
    }
}
