using Domain.Rider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.RiderConfig;

internal sealed class RiderProfileConfiguration : IEntityTypeConfiguration<RiderProfile>
{
    public void Configure(EntityTypeBuilder<RiderProfile> builder)
    {
        builder.ToTable("TBL_RIDER_PROFILE", t =>
        {
            t.HasCheckConstraint(
                "CK_RIDER_PROFILE_VEHICLE_TYPE",
                "\"vehicle_type\" IN ('Motorcycle', 'Bicycle', 'Car')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.NumberPlate)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.VehicleType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.VehicleMake)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.VehicleModel)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.VehicleColor)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.VehicleYear)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.DriverLicenseUrl)
            .HasMaxLength(500);

        builder.Property(x => x.DriverLicenseId)
            .HasMaxLength(200);

        builder.Property(x => x.NationalIdUrl)
            .HasMaxLength(500);

        builder.Property(x => x.NationalIdId)
            .HasMaxLength(200);

        builder.Property(x => x.VehiclePhotoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.VehiclePhotoId)
            .HasMaxLength(200);

        builder.Property(x => x.InsuranceCertUrl)
            .HasMaxLength(500);

        builder.Property(x => x.InsuranceCertId)
            .HasMaxLength(200);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(2000);

        builder.Property(x => x.VerifiedAt);

        builder.Property(x => x.VerifiedBy);

        // Relationships

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<RiderProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId)
           .IsUnique();

        builder.HasIndex(x => x.NumberPlate)
            .IsUnique();

        builder.HasQueryFilter(r => !r.IsSoftDeleted);

    }
}
