using Application.Abstractions.Messaging;
using Application.Orders.Dto;
using SharedKernel;

namespace Application.Orders.GetRestaurantOrders;

public sealed record GetRestaurantOrdersQuery(Guid RestaurantId, string? Status, int Page = 1, int PageSize = 20) : IQuery<PaginatedResult<OrderSummaryDto>>;
