using Domain.Payment;
using Domain.Rider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.RiderConfig;
internal sealed class RiderVerificationStatusConfiguration : IEntityTypeConfiguration<RiderVerificationStatus>
{
    public void Configure(EntityTypeBuilder<RiderVerificationStatus> builder)
    {
        builder.ToTable("TBL_RIDER_VERIFICATION_STATUS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(500);

        builder.HasData(RiderVerificationStatus.GetValues());
    }
}
