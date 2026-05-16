using Application.Abstractions.Messaging;

namespace Application.Notification.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(Guid? NotificationId = null) : ICommand;
