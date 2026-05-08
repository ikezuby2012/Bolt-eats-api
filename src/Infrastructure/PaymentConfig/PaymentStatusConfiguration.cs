using Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.PaymentConfig;

internal sealed class PaymentStatusConfiguration : IEntityTypeConfiguration<Domain.Payment.PaymentStatus>
{
    public void Configure(EntityTypeBuilder<PaymentStatus> builder)
    {
        builder.ToTable("TBL_PAYMENT_STATUS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(500);

        builder.HasData(PaymentStatus.GetValues());
    }
}
