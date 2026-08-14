using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using Domain.Common;
using Domain.Rider;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.SubmitRiderProfile;

internal sealed class SubmitRiderProfileCommandHandler(IApplicationDbContext db, IDateTimeProvider dateTimeProvider) : ICommandHandler<SubmitRiderProfileCommand, RiderProfileDto>
{
    public async Task<Result<RiderProfileDto>> Handle(SubmitRiderProfileCommand command, CancellationToken cancellationToken)
    {
        Domain.Users.User? user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<RiderProfileDto>(
                CommonErrors.CustomErrorMessage("User not found."));
        }

        RiderProfile? existing = await db.RiderProfiles
           .FirstOrDefaultAsync(r => r.UserId == command.UserId, cancellationToken);

        if (existing?.StatusId == RiderVerificationStatus.Verified.Id)
        {
            return Result.Failure<RiderProfileDto>(
               CommonErrors.CustomErrorMessage(
                   "Your profile is already verified. Contact support to make changes."));
        }

        if (existing is null)
        {
            existing = new RiderProfile
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                CreatedAt = dateTimeProvider.UtcNow,
                CreatedBy = command.UserId.ToString()
            };
            db.RiderProfiles.Add(existing);
        }

        user.RoleId = UserRole.Rider.Id;

        existing.NumberPlate = command.NumberPlate.ToUpperInvariant();
        existing.VehicleType = command.VehicleType;
        existing.VehicleMake = command.VehicleMake;
        existing.VehicleModel = command.VehicleModel;
        existing.VehicleColor = command.VehicleColor;
        existing.VehicleYear = command.VehicleYear;
        existing.StatusId = RiderVerificationStatus.Pending.Id;
        existing.RejectionReason = null;
        existing.UpdatedAt = dateTimeProvider.UtcNow;
        existing.UpdatedBy = command.UserId.ToString();

        if (command.InsuranceCertLink is not null)
        { existing.InsuranceCertUrl = command.InsuranceCertLink; }
        if (command.DriverLicenseLink is not null)
        { existing.DriverLicenseUrl = command.DriverLicenseLink; }
        if (command.VehiclePhotoLink is not null)
        { existing.VehiclePhotoUrl = command.VehiclePhotoLink; }
        if (command.NationalIdLink is not null)
        { existing.NationalIdUrl = command.NationalIdLink; }

        await db.SaveChangesAsync(cancellationToken);

        return (RiderProfileDto)existing;
    }
}
