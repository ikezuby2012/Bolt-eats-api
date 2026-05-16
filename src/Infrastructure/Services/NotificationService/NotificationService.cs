using System.Threading.Channels;
using Application.Abstractions.Data;
using Application.Abstractions.Services.Notification;
using Domain.Notification;

namespace Infrastructure.Services.NotificationService;

internal sealed class NotificationService(IApplicationDbContext db, IPushNotificationService push) : INotificationService
{
    public async Task NotifyAsync(Guid userId, int NotificationTypeId, int NotificationChannelId, string title, string body, object? payload = null, CancellationToken cancellationToken = default)
    {
        string? payloadJson = payload is not null
            ? System.Text.Json.JsonSerializer.Serialize(payload)
            : null;

        if (NotificationChannelId == NotificationChannel.InApp.Id || NotificationChannelId == NotificationChannel.Both.Id)
        {
            db.Notification.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationTypeId = NotificationTypeId,
                NotificationChannelId = NotificationChannelId,
                Title = title,
                Body = body,
                Payload = payloadJson,
                IsRead = false
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        if (NotificationChannelId == NotificationChannel.Push.Id || NotificationChannelId == NotificationChannel.Both.Id)
        {
            await push.SendToUserAsync(
              userId, title, body, NotificationTypeId, payload, cancellationToken);
        }

    }
}
