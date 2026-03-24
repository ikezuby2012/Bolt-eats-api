using Domain.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.OrderConfig;

internal sealed class EOrderStatusConfiguration : IEntityTypeConfiguration<EOrderStatus>
{
    public void Configure(EntityTypeBuilder<EOrderStatus> builder)
    {
        builder.ToTable("tbl_order_status");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(500);

        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);

        builder.HasData(EOrderStatus.GetValues());
    }
}
