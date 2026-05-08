using Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.PaymentConfig;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("TBL_PAYMENTS");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Amount)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.GatewayReference)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.ClientSecret)
            .HasMaxLength(500);

        builder.Property(p => p.GatewayCustomerId)
            .HasMaxLength(200);

        builder.Property(p => p.FailureCode)
            .HasMaxLength(100);

        builder.Property(p => p.FailureMessage)
            .HasMaxLength(500);

        builder.Property(p => p.RefundReference)
            .HasMaxLength(200);
        builder.Property(p => p.RefundAmount)
            .HasPrecision(18, 2);

        builder.Property(P => P.AmountInKobo).HasPrecision(18, 2);

        builder.HasOne(p => p.Order)
           .WithMany() // or .WithMany(o => o.Payments) if navigation exists
           .HasForeignKey(p => p.OrderId)
           .OnDelete(DeleteBehavior.Restrict);

        // Payment → Customer (User)
        builder.HasOne(p => p.Customer)
            .WithMany() // or .WithMany(u => u.Payments)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Gateway)
            .WithMany()
            .HasForeignKey(p => p.GatewayId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payment → Status
        builder.HasOne(p => p.Status)
            .WithMany()
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.CustomerId);
        builder.HasIndex(p => p.GatewayReference)
            .IsUnique();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        builder.HasQueryFilter(r => !r.IsSoftDeleted);
    }
}
