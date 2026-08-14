using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Notification;
using Application.Users.Dto;
using Domain.Common;
using Domain.Notification;
using Domain.Rider;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.ReviewRiderProfileCommand;

internal sealed class ReviewRiderProfileCommandHandler(IApplicationDbContext db, IDateTimeProvider dateTimeProvider, INotificationService notificationService) : ICommandHandler<ReviewRiderProfileCommand, RiderProfileDto>
{
    public async Task<Result<RiderProfileDto>> Handle(ReviewRiderProfileCommand command, CancellationToken cancellationToken)
    {
        Domain.Rider.RiderProfile? profile = await db.RiderProfiles
            .Include(r => r.User)
            .FirstOrDefaultAsync(
                r => r.Id == command.RiderProfileId, cancellationToken);

        if (profile is null)
        {
            return Result.Failure<RiderProfileDto>(
               CommonErrors.CustomErrorMessage("Rider profile not found."));
        }

        if (command.Approved)
        {
            profile.StatusId = RiderVerificationStatus.Verified.Id;
            profile.VerifiedAt = dateTimeProvider.UtcNow;
            profile.VerifiedBy = command.ReviewedBy;
            profile.RejectionReason = null;

            await notificationService.NotifyAsync(
                userId: profile.UserId,
                NotificationTypeId: NotificationType.General.Id,
                NotificationChannelId: NotificationChannel.Both.Id,
                title: "Profile Verified ✅",
                body: "Your rider profile has been verified. You can now accept orders!",
                payload: new { screen = "RiderDashboard" },
                cancellationToken: cancellationToken);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(command.RejectionReason))
            {
                return Result.Failure<RiderProfileDto>(
                    CommonErrors.CustomErrorMessage(
                        "Rejection reason is required."));
            }


            profile.StatusId = RiderVerificationStatus.Rejected.Id;
            profile.RejectionReason = command.RejectionReason;

            await notificationService.NotifyAsync(
                userId: profile.UserId,
                NotificationTypeId: NotificationType.General.Id,
                NotificationChannelId: NotificationChannel.Both.Id,
                title: "Profile Needs Update ⚠️",
                body: $"Your rider profile was not approved: {command.RejectionReason}",
                payload: new { screen = "RiderProfileForm" },
                cancellationToken: cancellationToken);
        }

        profile.UpdatedAt = dateTimeProvider.UtcNow;
        profile.UpdatedBy = command.ReviewedBy.ToString();

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success((RiderProfileDto)profile);
    }
}
