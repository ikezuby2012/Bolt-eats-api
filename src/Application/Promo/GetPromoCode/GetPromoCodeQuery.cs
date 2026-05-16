using Application.Abstractions.Messaging;
using Application.Promo.Dto;
using SharedKernel;

namespace Application.Promo.GetPromoCode;

public sealed record GetPromoCodeQuery(
    bool? ActiveOnly = null,
    Guid? RestaurantId = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<PromoCodeDto>>;
