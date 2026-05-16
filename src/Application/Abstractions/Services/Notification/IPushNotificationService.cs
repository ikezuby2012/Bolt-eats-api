using Domain.Notification;

namespace Application.Abstractions.Services.Notification;

public interface IPushNotificationService
{
    Task SendToUserAsync(
        Guid userId,
        string title,
        string body,
        int NotificationtypeId,
        object? payload = null,
        CancellationToken cancellationToken = default);
}
