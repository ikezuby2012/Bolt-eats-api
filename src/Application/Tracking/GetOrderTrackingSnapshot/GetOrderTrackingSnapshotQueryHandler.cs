using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Tracking.Dto;
using Application.Users.Dto;
using Domain.Order;
using Domain.Rider;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tracking.GetOrderTrackingSnapshot;

internal sealed class GetOrderTrackingSnapshotQueryHandler(IApplicationDbContext db, IUserContext userContext) : IQueryHandler<GetOrderTrackingSnapshotQuery, OrderTrackingSnapshotDto>
{
    public async Task<Result<OrderTrackingSnapshotDto>> Handle(GetOrderTrackingSnapshotQuery query, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.Order.Order? order = await db.Order.AsNoTracking().Include(o => o.Address).FirstOrDefaultAsync(o => o.Id == query.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderTrackingSnapshotDto>(Domain.Common.CommonErrors.CustomErrorMessage("Order not found!"));
        }

        if (order.CustomerId != userId)
        {
            return Result.Failure<OrderTrackingSnapshotDto>(Domain.Common.CommonErrors.CustomErrorMessage("Order not found!"));
        }

        int[] trackableStatuses = new[]
        {
            EOrderStatus.ReadyForPickup.Id,
            EOrderStatus.InTransit.Id,
            EOrderStatus.Delivered.Id
        };

        if (!trackableStatuses.Contains(order.OrderStatusId))
        {
            return Result.Failure<OrderTrackingSnapshotDto>(Domain.Common.CommonErrors.CustomErrorMessage($"Order is not yet out for delivery. Current status: {EOrderStatus.FromValue(order.OrderStatusId)!.Name}."));
        }

        Domain.Rider.RiderLocation? location = await db.RiderLocations.AsNoTracking().Where(r => r.OrderId == query.OrderId).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);

        int? eta = EstimatedMinutesRemaining(order, location);
        var riderLocation = (RiderLocationDto)location!;
        var riderAddress = (AddressDto)order.Address;

        return Result.Success(new OrderTrackingSnapshotDto(
            OrderId: order.Id,
            Status: EOrderStatus.FromValue(order.OrderStatusId)!.Name,
            RiderLocation: riderLocation,
            DeliveryAddress: riderAddress,
            EstimatedMinutesRemaining: eta));
    }

    private static int? EstimatedMinutesRemaining(Order order, RiderLocation? location)
    {
        if (order.OrderStatusId == EOrderStatus.Delivered.Id)
        {
            return 0;
        }

        if (order.OrderStatusId == EOrderStatus.InTransit.Id && order.PickedUpAt.HasValue)
        {
            int elapsedSincePickup = (int)(DateTime.UtcNow - order.PickedUpAt.Value).TotalMinutes;

            int travelOnly = order.EstimatedTravelMinutes
                      ?? order.EstimatedDeliveryMinutes
                      ?? 30;

            return Math.Max(0, travelOnly - elapsedSincePickup);
        }

        if (location is null)
        {
            return order.EstimatedDeliveryMinutes;
        }

        TimeSpan? pingAge = DateTime.UtcNow - location.CreatedAt;
        return pingAge > TimeSpan.FromMinutes(5)
            ? null   // stale — don't show a stale ETA
            : order.EstimatedDeliveryMinutes;
    }
}
