using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Notification;
using Application.Abstractions.Services.Order;
using Domain.Common;
using Domain.Notification;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.OfferOrderToRider;
internal sealed class OfferOrderToRiderCommandHandler(IApplicationDbContext db, IOrderHubService orderHubService, INotificationService notificationService, IDateTimeProvider dateTimeProvider) : ICommandHandler<OfferOrderToRiderCommand>
{
    private static readonly TimeSpan OfferWindow = TimeSpan.FromSeconds(30);
    public async Task<Result> Handle(OfferOrderToRiderCommand command, CancellationToken cancellationToken)
    {
        Domain.Order.Order? order = await db.Order
           .Include(o => o.Restaurant)
           .Include(o => o.Items)
           .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(CommonErrors.CustomErrorMessage("Order not found."));
        }

        order.OfferedToRiderId = command.RiderId;
        //order.RiderId = command.RiderId;
        order.OfferedAt = dateTimeProvider.UtcNow.Add(OfferWindow);

        await db.SaveChangesAsync(cancellationToken);

        await orderHubService.NotifyRiderAssignedAsync(
            command.RiderId,
            new
            {
                type = "OrderOffer",
                orderId = order.Id.ToString(),
                restaurantName = order.Restaurant.Name,
                restaurantLat = order.Restaurant.Addresses.First().Latitude,
                restaurantLng = order.Restaurant.Addresses.First().Longitude,
                deliveryLat = order.Address.Latitude,
                deliveryLng = order.Address.Longitude,
                total = order.Total,
                itemCount = order.Items.Count,
                estimatedEta = order.EstimatedDeliveryMinutes,
                expiresInSeconds = (int)OfferWindow.TotalSeconds
            },
            cancellationToken);

        await notificationService.NotifyAsync(
            userId: command.RiderId,
            NotificationTypeId: NotificationType.OrderConfirmed.Id,
            NotificationChannelId: NotificationChannel.Push.Id,
            title: "New Delivery Request 🛵",
            body: $"{order.Restaurant.Name} — ₦{order.Total:N0} — {order.Items.Count} items",
            payload: new { screen = "IncomingOrder", orderId = order.Id },
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
