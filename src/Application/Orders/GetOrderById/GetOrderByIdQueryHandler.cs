using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.Dto;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetOrderById;

internal sealed class GetOrderByIdQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await context.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<OrderDto>(UserErrors.NotFound(userId));
        }

        Domain.Order.Order? order = await context.Order
            .AsNoTracking()
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Include(o => o.Rider)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == query.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDto>(Domain.Common.CommonErrors.CustomErrorMessage("Order not found."));
        }

        /// Owners can only see orders for their restaurants
        bool isRestaurantOwner = await context.Restaurants.AnyAsync(r => r.Id == order.RestaurantId && r.CreatedBy == userId.ToString(), cancellationToken);

        bool isCustomer = order.CustomerId == userId;

        // Optional (only if exists)
        bool isOrderCreator = order.CreatedBy == userId.ToString();

        // Final authorization check
        if (!(isRestaurantOwner || isCustomer || isOrderCreator))
        {
            return Result.Failure<OrderDto>(
                Domain.Common.CommonErrors.CustomErrorMessage("Order not found.")
            );
        }

        return (OrderDto)order;
    }
}
