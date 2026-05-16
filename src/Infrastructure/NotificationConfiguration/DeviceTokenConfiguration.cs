using Domain.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.NotificationConfiguration;
internal sealed class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("TBL_DEVICE_TOKEN");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id).HasDefaultValueSql("GEN_RANDOM_UUID()").IsRequired();

        builder.HasIndex(d => d.Token).IsUnique();

        builder.Property(d => d.Token).IsRequired().HasMaxLength(500);
        builder.Property(d => d.Platform).IsRequired().HasMaxLength(20);

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
