using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.Dto;
using Domain.Order;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetAllOrdersAdmin;

internal sealed class GetAllOrdersAdminQueryHandler(IApplicationDbContext context) : IQueryHandler<GetAllOrdersAdminQuery, PaginatedResult<OrderSummaryDto>>
{
    public async Task<Result<PaginatedResult<OrderSummaryDto>>> Handle(GetAllOrdersAdminQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = context.Order.AsNoTracking().Where(x => (!request.RestaurantId.HasValue || x.RestaurantId == request.RestaurantId) &&
                                   (!request.CustomerId.HasValue || x.CustomerId == request.CustomerId) && (!request.RiderId.HasValue || x.RiderId == request.RiderId) && (!request.From.HasValue || x.CreatedAt >= request.From.Value)
                            && (!request.To.HasValue || x.CreatedAt <= request.To.Value));

        if (!string.IsNullOrEmpty(request.statusFilter) && EOrderStatus.IsValidName(request.statusFilter))
        {
            int statusId = EOrderStatus.FromName(request.statusFilter)!.Id;
            query = query.Where(o => o.OrderStatusId == statusId);
        }
        query = query.OrderByDescending(o => o.CreatedAt);

        int total = await query.CountAsync(cancellationToken);

        List<OrderSummaryDto> items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto(
                   o.Id,
                o.OrderCode,
                o.RestaurantId,
                o.Restaurant.Name,
                o.Restaurant.LogoUrl ?? "",
                o.OrderStatusId,
                o.EstimatedDeliveryMinutes,
                EOrderStatus.FromValue(o.OrderStatusId)!.Name,
                o.Total,
                o.Items.Count,
                o.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<OrderSummaryDto>
        {
            Data = items,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalItems = total
        };
    }
}
