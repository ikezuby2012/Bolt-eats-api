using Domain.Notification;

namespace Application.Abstractions.Services.Notification;

public interface INotificationService
{
    Task NotifyAsync(
        Guid userId,
        int NotificationTypeId,
        int NotificationChannelId,
        string title,
        string body,
        object? payload = null,
        CancellationToken cancellationToken = default);
}
