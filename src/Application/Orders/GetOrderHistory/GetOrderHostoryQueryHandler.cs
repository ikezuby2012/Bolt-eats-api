using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.Dto;
using Domain.Order;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.GetOrderHistory;

internal sealed class GetOrderHostoryQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetOrderHistoryQuery, PaginatedResult<OrderSummaryDto>>
{
    public async Task<Result<PaginatedResult<OrderSummaryDto>>> Handle(GetOrderHistoryQuery req, CancellationToken cancellationToken)
    {
        Guid customerId = userContext.UserId;

        IQueryable<Domain.Order.Order> query = context.Order.AsNoTracking().AsQueryable().Include(x => x.Address).Where(x => x.CustomerId == customerId
                           && (!req.DateFrom.HasValue || x.CreatedAt >= req.DateFrom.Value)
                           && (!req.DateTo.HasValue || x.CreatedAt <= req.DateTo.Value));

        int totalItems = await query.CountAsync(cancellationToken);

        List<OrderSummaryDto> items = await query.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.RestaurantId,
                o.Restaurant.Name,
                EOrderStatus.FromValue(o.OrderStatusId)!.Name,
                o.Total,
                o.Items.Count,
                o.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<OrderSummaryDto>
        {
            Data = items,
            TotalItems = totalItems,
            PageNumber = req.Page,
            PageSize = req.PageSize,
        };
    }
}
