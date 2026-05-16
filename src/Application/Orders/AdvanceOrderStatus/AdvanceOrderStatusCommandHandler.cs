using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.Dto;
using Domain.Common;
using Domain.Notification;
using Domain.Order;
using Domain.Rider;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.AdvanceOrderStatus;

internal sealed class AdvanceOrderStatusCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<AdvanceOrderStatusCommand, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(AdvanceOrderStatusCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Order.Order? order = await context.Order
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Include(o => o.Rider)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDto>(CommonErrors.CustomErrorMessage("No Order was found"));
        }

        /// check if its the owner
        Domain.Users.User? customer = await context.Users
           .Include(u => u.Addresses)
           .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure<OrderDto>(CommonErrors.CustomErrorMessage("No User was found"));
        }

        if (customer.RoleId == UserRole.BusinessOwner.Id)
        {
            bool ownsRestaurant = await context.Restaurants.AnyAsync(
                r => r.Id == order.RestaurantId && r.OwnerId == userId,
                cancellationToken);

            if (!ownsRestaurant)
            {
                return Result.Failure<OrderDto>(CommonErrors.CustomErrorMessage("No Order was found"));
            }
        }

        if (customer.RoleId == UserRole.Rider.Id && order.RiderId != userId)
        {
            return Result.Failure<OrderDto>(CommonErrors.CustomErrorMessage("this order is not assigned to you."));
        }
        string userRole = UserRole.FromValue(customer.RoleId)!.Name;
        var orderStatus = EOrderStatus.FromName(command.Status);

        string oldStatus = EOrderStatus.FromValue(order.OrderStatusId)!.Name;
        EOrderStatus newStatus = EOrderStatus.FromNameOrDefault(command.Status)!;

        if (!OrderStateMachine.CanTransition(userRole, EOrderStatus.FromValue(order.OrderStatusId)!, orderStatus!))
        {
            var allowed = OrderStateMachine
               .AllowedNext(userRole, EOrderStatus.FromValue(order.OrderStatusId)!)
               .ToList();

            string hint = allowed.Any()
                ? $" Allowed next: {string.Join(", ", allowed)}."
                : " No further transitions are allowed from this status.";

            return Result.Failure<OrderDto>(CommonErrors.CustomErrorMessage($"Cannot transition from {oldStatus} to {newStatus.Name}.{hint}"));
        }

        order.OrderStatusId = EOrderStatus.FromName(command.Status)!.Id;

        switch (orderStatus)
        {
            case EOrderStatus status when status == EOrderStatus.Accepted:
                order.AcceptedAt = DateTime.UtcNow;
                break;
            //case EOrderStatus status when status == EOrderStatus.Preparing:
            //    order.PreparingAt = DateTime.UtcNow;
            //    break;
            case EOrderStatus status when status == EOrderStatus.ReadyForPickup:
                order.PickedUpAt = DateTime.UtcNow;
                break;
            case EOrderStatus status when status == EOrderStatus.Refunded:
                order.RefundedAt = DateTime.UtcNow;
                break;
            case EOrderStatus status when status == EOrderStatus.Delivered:
                order.DeliveredAt = DateTime.UtcNow;
                break;
        }

        (string title, string body) = newStatus.Id switch
        {
            var id when id == EOrderStatus.Accepted.Id =>
                ("Order Confirmed", "Your order has been accepted."),

            var id when id == EOrderStatus.Preparing.Id =>
                ("Being Prepared", "The restaurant is preparing your order."),

            var id when id == EOrderStatus.ReadyForPickup.Id =>
                ("Rider On the Way", "A rider is heading to pick up your order."),

            var id when id == EOrderStatus.InTransit.Id =>
                ("Out for Delivery", "Your order is on its way."),

            var id when id == EOrderStatus.Delivered.Id =>
                ("Delivered", "Enjoy your meal! Leave a review?"),

            var id when id == EOrderStatus.Cancelled.Id =>
                ("Order Cancelled", "Your order has been cancelled."),

            _ => (string.Empty, string.Empty)
        };

        order.Raise(new BroadcastStatusChangeDomain(
             Id: order.Id,
             payload: new OrderStatusChanged(
                 order.Id,
                 oldStatus,
                 newStatus.Name,
                 ChangedAt: DateTime.UtcNow),

             userId: order.CustomerId,
             NotificationTypeId: MapToNotificationType(newStatus.Id),
             NotificationChannelId: NotificationChannel.Both.Id,
             title: title,
             body: body,
             notifyPayload: new
             {
                 screen = "OrderDetail",
                 orderId = order.Id
             }
         ));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success((OrderDto)order);
    }

    private int MapToNotificationType(int Id)
    {
        return Id switch
        {
            var id when id == EOrderStatus.Accepted.Id => NotificationType.OrderConfirmed.Id,
            var id when id == EOrderStatus.Preparing.Id => NotificationType.OrderPreparing.Id,
            var id when id == EOrderStatus.ReadyForPickup.Id => NotificationType.OrderReadyForPickup.Id,
            var id when id == EOrderStatus.InTransit.Id => NotificationType.OrderOutForDelivery.Id,
            var id when id == EOrderStatus.Delivered.Id => NotificationType.OrderDelivered.Id,
            var id when id == EOrderStatus.Cancelled.Id => NotificationType.OrderCancelled.Id,
            _ => 0
        };
    }
}

