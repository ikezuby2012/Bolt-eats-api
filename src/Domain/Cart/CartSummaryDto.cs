namespace Domain.Cart;

public record CartSummaryDto(
    decimal Subtotal,
    decimal DeliveryFee,
    decimal PromoDiscount,
    decimal Tax,
    decimal Total,
    string? PromoCode,
    bool FreeDelivery,
    bool MeetMinimumOrder);
