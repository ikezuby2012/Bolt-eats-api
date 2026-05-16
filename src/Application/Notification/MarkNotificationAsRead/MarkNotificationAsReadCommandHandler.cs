using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Notification.MarkNotificationAsRead;

internal sealed class MarkNotificationAsReadCommandHandler(IApplicationDbContext db, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<MarkNotificationAsReadCommand>
{
    public async Task<Result> Handle(MarkNotificationAsReadCommand command, CancellationToken cancellationToken)
    {
        DateTime now = dateTimeProvider.UtcNow;
        Guid userId = userContext.UserId;

        await db.Notification
             .Where(n => n.UserId == userId && !n.IsRead)
             .ExecuteUpdateAsync(
                 s => s
                     .SetProperty(n => n.IsRead, true)
                     .SetProperty(n => n.ReadAt, now),
                 cancellationToken);

        return Result.Success();
    }
}
