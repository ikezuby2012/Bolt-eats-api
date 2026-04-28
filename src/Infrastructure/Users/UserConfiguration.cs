using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("TBL_USERS");

        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(256)
               .HasColumnName("EMAIL");

        builder.Property(u => u.FirstName)
                       .HasMaxLength(100)
                       .HasColumnName("FIRST_NAME");

        builder.Property(u => u.LastName)
               .HasMaxLength(100)
               .HasColumnName("LAST_NAME");

        builder.Property(u => u.OTP)
               .HasMaxLength(6)
               .HasColumnName("OTP");

        builder.Property(u => u.PasswordHash)
               .IsRequired()
               .HasMaxLength(512)
               .HasColumnName("PASSWORD_HASH");

        builder.Property(u => u.PhoneNumber).HasMaxLength(20).HasColumnName("phone_number");
        builder.Property(u => u.DateOfBirth).HasColumnName("date_of_birth");
        builder.Property(u => u.ProfileImageUrl).HasMaxLength(2000).HasColumnName("profile_image_url");

        builder.Property(u => u.CreatedAt)
               .HasColumnName("CREATED_AT");

        builder.Property(u => u.CreatedBy)
               .HasMaxLength(128)
               .HasColumnName("CREATED_BY");

        builder.Property(u => u.UpdatedAt)
               .HasColumnName("UPDATED_AT");

        builder.Property(u => u.UpdatedBy)
               .HasMaxLength(128)
               .HasColumnName("UPDATED_BY");

        builder.Property(u => u.LastLogin)
               .HasColumnName("LAST_LOGIN");

        builder.Property(u => u.RoleId)
               .HasColumnName("ROLE_ID").HasDefaultValue(1);

        builder.Property(u => u.isVerifed)
               .HasColumnName("IS_VERIFIED")
               .HasDefaultValue(false);

        builder.Property(u => u.IsActive)
              .HasColumnName("IS_ACTIVE")
              .HasDefaultValue(true);

        builder.Property(u => u.IsOnline).HasColumnName("IS_ONLINE").HasDefaultValue(false);

        builder.Property(u => u.IsSoftDeleted)
                .HasColumnName("IS_SOFT_DELETED")
                .HasDefaultValue(false);

        builder.HasMany(u => u.Addresses)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_user_addresses");

        builder.HasOne(u => u.UserRole)
               .WithMany()
               .HasForeignKey(u => u.RoleId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
