using System.Text.Json.Serialization;
using SharedKernel;

namespace Application.Notification.Dto;

public sealed class NotificationDto : IAuditable<Guid>
{
    public Guid Id { get; set; }
    public string NotificationType { get; set; }
    public string NotificationChannel { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string? Payload { get; set; }
    public bool IsRead { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    [JsonIgnore]
    public bool IsSoftDeleted { get; set; }

    public static explicit operator NotificationDto(Domain.Notification.Notification notification) => new NotificationDto
    {
        Id = notification.Id,
        NotificationType = Domain.Notification.NotificationType.FromValue(notification.NotificationTypeId)!.Name ?? "",
        NotificationChannel = Domain.Notification.NotificationChannel.FromValue(notification.NotificationChannelId)!.Name ?? string.Empty,
        Title = notification.Title,
        Body = notification.Body,
        Payload = notification.Payload,
        CreatedAt = notification.CreatedAt,
        CreatedBy = notification.CreatedBy,
        UpdatedAt = notification.UpdatedAt,
        UpdatedBy = notification.UpdatedBy,
    };
}
