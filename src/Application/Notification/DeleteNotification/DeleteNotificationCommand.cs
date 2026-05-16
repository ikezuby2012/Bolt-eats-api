using Application.Abstractions.Messaging;

namespace Application.Notification.DeleteNotification;

public sealed record DeleteNotificationCommand(Guid NotificationId) : ICommand;
