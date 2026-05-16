using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Notification.DeleteNotification;

internal sealed class DeleteNotificationCommandHandler(IApplicationDbContext db, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<DeleteNotificationCommand>
{
    public async Task<Result> Handle(DeleteNotificationCommand command, CancellationToken cancellationToken)
    {
        DateTime now = dateTimeProvider.UtcNow;
        Guid userId = userContext.UserId;

        int deleted = await db.Notification
             .Where(n => n.UserId == userId && n.Id == command.NotificationId)
             .ExecuteUpdateAsync(
                 s => s
                     .SetProperty(n => n.IsSoftDeleted, true)
                     .SetProperty(n => n.UpdatedAt, now),
                 cancellationToken);

        if (deleted == 0)
        {
            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("Notification not found"));
        }

        return Result.Success();
    }
}
