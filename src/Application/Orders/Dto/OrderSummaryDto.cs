namespace Application.Orders.Dto;

public record OrderSummaryDto(
    Guid Id,
    Guid RestaurantId,
    string RestaurantName,
    string Status,
    decimal Total,
    int ItemCount,
    DateTime? CreatedAt);
