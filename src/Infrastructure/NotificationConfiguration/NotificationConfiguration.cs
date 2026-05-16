using Domain.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.NotificationConfiguration;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("TBL_NOTIFICATION");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id).HasDefaultValueSql("GEN_RANDOM_UUID()").IsRequired();

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.NotificationTypeId)
            .IsRequired();

        builder.Property(n => n.NotificationChannelId)
            .IsRequired();

        builder.Property(n => n.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Body)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(n => n.Payload)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.ReadAt)
            .IsRequired(false);

        builder.HasOne(n => n.User)
            .WithMany()  // Or .WithMany(u => u.Notifications) if User has collection
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.NotificationType)
            .WithMany()
            .HasForeignKey(n => n.NotificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.NotificationChannel)
            .WithMany()
            .HasForeignKey(n => n.NotificationChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(n => !n.IsSoftDeleted);
    }
}
