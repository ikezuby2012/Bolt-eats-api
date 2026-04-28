using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.Dto;
using Domain.Order;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetRiderActiveOrder;

internal class GetRiderActiveOrderQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetRiderActiveOrderQuery, OrderDto>
{
    private static readonly int[] ActiveRiderStatuses = [EOrderStatus.ReadyForPickup.Id, EOrderStatus.Cancelled.Id];
    public async Task<Result<OrderDto>> Handle(GetRiderActiveOrderQuery query, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<OrderDto>(UserErrors.NotFound(userId));
        }
        if (user.RoleId != Domain.Users.UserRole.Admin.Id)
        {
            return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage("Order not found"));
        }

        Order? order = await context.Order
            .AsNoTracking()
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Include(o => o.Rider)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(
                o => o.RiderId == userId &&
                     ActiveRiderStatuses.Contains(o.OrderStatusId),
                cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage("No active order assigned."));
        }

        return (OrderDto)order;
    }
}
