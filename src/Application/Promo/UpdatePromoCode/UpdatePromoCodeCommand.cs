using Application.Abstractions.Messaging;
using Application.Promo.Dto;

namespace Application.Promo.UpdatePromoCode;

public sealed record UpdatePromoCodeCommand(
     Guid Id,
    string? Description,
    decimal? MinOrderAmount,
    decimal? MaxDiscountCap,
    int? UsageLimitTotal,
    int? UsageLimitPerUser,
    DateTime? StartsAt,
    DateTime? ExpiresAt
    ) : ICommand<PromoCodeDto>;
