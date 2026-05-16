using Domain.Users;
using SharedKernel;

namespace Domain.Notification;

public sealed class Notification : Auditable<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public int NotificationTypeId { get; set; }
    public NotificationType NotificationType { get; set; }
    public int NotificationChannelId { get; set; }
    public NotificationChannel NotificationChannel { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string? Payload { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
