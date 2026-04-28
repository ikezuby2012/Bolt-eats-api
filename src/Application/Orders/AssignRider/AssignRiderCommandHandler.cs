using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.Dto;
using Domain.Common;
using Domain.Order;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.AssignRider;

internal sealed class AssignRiderCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<AssignRiderCommand, OrderDto>
{
    private static readonly EOrderStatus[] AssignableStatuses =
    [
        EOrderStatus.Pending,
        EOrderStatus.Accepted,
        EOrderStatus.Preparing,
        EOrderStatus.ReadyForPickup
    ];

    public async Task<Result<OrderDto>> Handle(AssignRiderCommand command, CancellationToken cancellationToken)
    {

        Guid userId = userContext.UserId;
        Order? order = await context.Order
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Include(o => o.Rider)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDto>(CommonErrors.CustomErrorMessage("No Order was found"));
        }
        EOrderStatus status = EOrderStatus.FromValue(order.OrderStatusId)!;

        if (!AssignableStatuses.Contains(status))
        {
            return Result.Failure<OrderDto>(CommonErrors.CustomErrorMessage($"Cannot assign a rider to an order with status {status.Name}."));
        }

        User? rider = await context.Users
             .FirstOrDefaultAsync(
                 u => u.Id == userId && u.RoleId == UserRole.Rider.Id && u.IsActive,
                 cancellationToken);

        if (rider is null)
        {
            return Result.Failure<OrderDto>(CommonErrors.CustomErrorMessage("Rider not found or inactive"));
        }

        // Check rider doesn't already have an active delivery
        bool riderBusy = await context.Order.AnyAsync(
            o => o.RiderId == command.RiderId &&
                 (o.OrderStatusId == EOrderStatus.InTransit.Id ||
                  o.OrderStatusId == EOrderStatus.ReadyForPickup.Id),
            cancellationToken);

        if (riderBusy)
        {
            return Result.Failure<OrderDto>(CommonErrors.CustomErrorMessage("Rider already has an active delivery"));
        }

        order.RiderId = command.RiderId;

        await context.SaveChangesAsync(cancellationToken);

        return (OrderDto)order;
    }
}
