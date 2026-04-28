using Application.Abstractions.Messaging;
using Application.Orders.Dto;
using SharedKernel;

namespace Application.Orders.GetAllOrdersAdmin;

public record GetAllOrdersAdminQuery(
    string? statusFilter,
    Guid? RestaurantId,
    Guid? CustomerId,
    Guid? RiderId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<OrderSummaryDto>>;
