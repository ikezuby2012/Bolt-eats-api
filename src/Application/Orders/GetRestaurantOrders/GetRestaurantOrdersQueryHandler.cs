using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.Dto;
using Domain.Order;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetRestaurantOrders;

internal class GetRestaurantOrdersQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetRestaurantOrdersQuery, PaginatedResult<OrderSummaryDto>>
{
    public async Task<Result<PaginatedResult<OrderSummaryDto>>> Handle(GetRestaurantOrdersQuery query, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        bool ownsRestaurant = await context.Restaurants.AnyAsync(r => r.Id == query.RestaurantId && r.CreatedBy == userId.ToString(), cancellationToken);

        if (!ownsRestaurant)
        {
            return Result.Failure<PaginatedResult<OrderSummaryDto>>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant not found."));
        }

        IQueryable<Domain.Order.Order> baseQuery = context.Order
            .AsNoTracking()
            .Where(o => o.RestaurantId == query.RestaurantId);

        if (!string.IsNullOrEmpty(query.Status) && EOrderStatus.IsValidName(query.Status))
        {
            int statusId = EOrderStatus.FromName(query.Status)!.Id;
            baseQuery = baseQuery.Where(o => o.OrderStatusId == statusId);
        }

        baseQuery = baseQuery.OrderByDescending(o => o.CreatedAt);

        int total = await baseQuery.CountAsync(cancellationToken);

        List<OrderSummaryDto> items = await baseQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.RestaurantId,
                o.Restaurant.Name,
                query.Status ?? "",
                o.Total,
                o.Items.Count,
                o.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<OrderSummaryDto>
        {
            Data = items,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalItems = total
        };
    }
}
