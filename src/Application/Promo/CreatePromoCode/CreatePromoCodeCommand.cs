using Application.Abstractions.Messaging;
using Application.Promo.Dto;

namespace Application.Promo.CreatePromoCode;

public sealed record CreatePromoCodeCommand(
    string Code,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountCap,
    Guid? RestaurantId,
    int? UsageLimitTotal,
    int? UsageLimitPerUser,
    DateTime StartsAt,
    DateTime ExpiresAt
    ) : ICommand<PromoCodeDto>;
