namespace Application.Orders.Dto;

public record OrderSummaryDto(
    Guid Id,
    string OrderCode,
    Guid RestaurantId,
    string RestaurantName,
    string restaurantImgLink,
    int StatusId,
    int? etaMinutes,
    string Status,
    decimal Total,
    int ItemCount,
    DateTime? CreatedAt);
