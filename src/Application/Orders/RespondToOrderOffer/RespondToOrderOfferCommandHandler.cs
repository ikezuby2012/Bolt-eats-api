using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Notification;
using Application.Abstractions.Services.Order;
using Application.Abstractions.Services.Rider;
using Domain.Common;
using Domain.Notification;
using Domain.Order;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.RespondToOrderOffer;

internal sealed class RespondToOrderOfferCommandHandler(
     IApplicationDbContext db,
    IDateTimeProvider dateTimeProvider,
    IOrderHubService orderHubService,
    INotificationService notificationService,
    IRiderAssignmentService assignmentService
    ) : ICommandHandler<RespondToOrderOfferCommand>
{
    public async Task<Result> Handle(RespondToOrderOfferCommand command, CancellationToken cancellationToken)
    {
        Domain.Order.Order? order = await db.Order
            .Include(o => o.Restaurant)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(CommonErrors.CustomErrorMessage("Order not found."));
        }

        if (order.OfferedToRiderId != command.RiderId)
        {
            return Result.Failure(CommonErrors.CustomErrorMessage("This offer is not for you."));
        }

        if (command.Accepted)
        {
            // ── Rider accepted — confirm assignment ───────────────────────
            order.RiderId = command.RiderId;
            order.OfferedToRiderId = null;
            //order.OfferExpiresAt = null;
            order.OrderStatusId = EOrderStatus.InTransit.Id;
            order.PickedUpAt = dateTimeProvider.Now;

            await db.SaveChangesAsync(cancellationToken);

            // Update Redis — mark rider as busy
            await assignmentService.MarkRiderBusyAsync(command.RiderId, cancellationToken);

            // Notify customer — rider is on the way
            await notificationService.NotifyAsync(
                userId: order.CustomerId,
                NotificationTypeId: NotificationType.OrderReadyForPickup.Id,
                NotificationChannelId: NotificationChannel.Both.Id,
                title: "Rider Assigned 🛵",
                body: "A rider has accepted your order and is heading to the restaurant.",
                payload: new { screen = "TrackingPage", orderId = order.Id },
                cancellationToken: cancellationToken);

            // Notify all SignalR subscribers
            await orderHubService.NotifyOrderStatusChangedAsync(
                order.Id, order.CustomerId, order.RestaurantId,
                command.RiderId, EOrderStatus.FromValue(order.OrderStatusId)!.Name ?? "",
                "Rider Assigned", dateTimeProvider.UtcNow, cancellationToken);
        }
        else
        {
            // ── Rider declined — clear offer and try next candidate ───────
            order.OfferedToRiderId = null;
            //order.OfferExpiresAt = null;
            //order.DeclinedByRiderIds ??= [];
            //order.DeclinedByRiderIds.Add(command.RiderId);

            await db.SaveChangesAsync(cancellationToken);

            // Try next best candidate — exclude this rider
            _ = assignmentService.TryAutoAssignAsync(
                order.Id, cancellationToken);
        }

        return Result.Success();
    }
}
