using SharedKernel;

namespace Domain.Rider;

public sealed record BroadcastStatusChangeDomain(
        Guid Id,
        OrderStatusChanged payload,
        Guid userId,
        int NotificationTypeId,
        int NotificationChannelId,
        string title,
        string body,
        object? notifyPayload = null) : IDomainEvent;
