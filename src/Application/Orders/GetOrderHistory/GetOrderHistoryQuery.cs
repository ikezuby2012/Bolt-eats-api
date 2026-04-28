using Application.Abstractions.Messaging;
using Application.Orders.Dto;
using SharedKernel;

namespace Application.Orders.GetOrderHistory;

public sealed record GetOrderHistoryQuery(int Page = 1, int PageSize = 20, DateTime? DateFrom = null, DateTime? DateTo = null) : IQuery<PaginatedResult<OrderSummaryDto>>;
