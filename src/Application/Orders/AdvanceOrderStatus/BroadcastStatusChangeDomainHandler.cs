using Application.Abstractions.Services;
using Application.Abstractions.Services.Notification;
using Application.Tracking.Dto;
using Domain.Rider;
using SharedKernel;

namespace Application.Orders.AdvanceOrderStatus;
internal sealed class BroadcastStatusChangeDomainHandler(ITrackingService trackingService, INotificationService notificationService) : IDomainEventHandler<BroadcastStatusChangeDomain>
{
    public async Task Handle(
    BroadcastStatusChangeDomain domainEvent,
    CancellationToken cancellationToken)
    {
        var orderStatus = new OrderStatusChangedPayload(
            OrderId: domainEvent.payload.OrderId,
            OldStatus: domainEvent.payload.OldStatus,
            NewStatus: domainEvent.payload.NewStatus,
            ChangedAt: domainEvent.payload.ChangedAt);

        if (!string.IsNullOrWhiteSpace(domainEvent.title))
        {
            await notificationService.NotifyAsync(
                userId: domainEvent.userId,
                NotificationTypeId: domainEvent.NotificationTypeId,
                NotificationChannelId: domainEvent.NotificationChannelId,
                title: domainEvent.title,
                body: domainEvent.body,
                payload: domainEvent.notifyPayload,
                cancellationToken: cancellationToken);
        }

        await trackingService.BroadcastStatusChangeAsync(
            domainEvent.Id,
            orderStatus,
            cancellationToken);
    }
}
