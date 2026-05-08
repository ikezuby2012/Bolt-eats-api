using Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.PaymentConfig;

internal sealed class PaymentGatewayConfiguration : IEntityTypeConfiguration<Domain.Payment.PaymentGateway>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PaymentGateway> builder)
    {
        builder.ToTable("TBL_PAYMENT_GATEWAY");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(500);

        builder.HasData(PaymentGateway.GetValues());
    }
}
