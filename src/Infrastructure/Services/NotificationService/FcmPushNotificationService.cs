using Application.Abstractions.Data;
using Application.Abstractions.Services.Notification;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.NotificationService;

internal sealed class FcmPushNotificationService(IApplicationDbContext db) : IPushNotificationService
{
    private static readonly FirebaseMessaging Messaging =
        FirebaseMessaging.GetMessaging(FirebaseAdmin.FirebaseApp.DefaultInstance);

    public async Task SendToUserAsync(Guid userId, string title, string body, int NotificationtypeId, object? payload = null, CancellationToken cancellationToken = default)
    {
        List<string> tokens = await db.DeviceTokens
            .Where(d => d.UserId == userId && d.IsActive)
            .Select(d => d.Token)
            .ToListAsync(cancellationToken);

        if (!tokens.Any())
        {
            return;
        }

        var data = new Dictionary<string, string>
        {
            ["type"] = Domain.Notification.NotificationType.FromValue(NotificationtypeId)!.Name,
        };

        if (payload != null)
        {
            data["payload"] = System.Text.Json.JsonSerializer.Serialize(payload);
        }

        var messages = tokens.Select(token => new Message
        {
            Token = token,
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = title,
                Body = body
            },
            Data = data,
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "orders",
                    Sound = "default"
                }
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    Sound = "default",
                    ContentAvailable = true
                }
            }
        }).ToList();

        BatchResponse response = await Messaging.SendEachAsync(messages, cancellationToken);

        await DeactivateStaleTokensAsync(tokens, response, cancellationToken);
    }


    // FCM reports unregistered tokens per-message — deactivate them immediately
    private async Task DeactivateStaleTokensAsync(
        List<string> tokens,
        BatchResponse response,
        CancellationToken cancellationToken)
    {
        var stale = tokens
            .Zip(response.Responses, (token, result) => (token, result))
            .Where(x =>
                !x.result.IsSuccess &&
                x.result.Exception?.MessagingErrorCode
                    is MessagingErrorCode.Unregistered
                    or MessagingErrorCode.InvalidArgument)
            .Select(x => x.token)
            .ToList();

        if (!stale.Any())
        {
            return;
        }


        await db.DeviceTokens
            .Where(d => stale.Contains(d.Token))
            .ExecuteUpdateAsync(
                s => s.SetProperty(d => d.IsActive, false),
                cancellationToken);
    }
}
