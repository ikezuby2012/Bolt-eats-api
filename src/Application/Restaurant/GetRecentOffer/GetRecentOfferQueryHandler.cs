using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetRecentOffer;

internal sealed class GetRecentOfferQueryHandler(IApplicationDbContext db) : IQueryHandler<GetRecentOfferQuery, IReadOnlyList<HomeMenuItemDto>>
{
    public async Task<Result<IReadOnlyList<HomeMenuItemDto>>> Handle(GetRecentOfferQuery query, CancellationToken cancellationToken)
    {
        return await db.MenuItem
             .AsNoTracking()
             .Where(m => m.IsAvailable &&
                         m.DiscountPrice != null &&
                         m.DiscountPrice > 0 &&
                         m.Restaurant.IsActive &&
                         m.Restaurant.IsOpen)
             .OrderByDescending(m => m.CreatedAt)
             .Take(query.Limit)
             .Select(m => new HomeMenuItemDto(
                m.Id,
                m.Name,
                m.ImageUrl,
                m.Price,
                m.DiscountPrice,
                m.PrepTimeMin,
                m.RestaurantId,
                m.Restaurant.Name,
                m.Restaurant.LogoUrl,
                m.Restaurant.Rating,
                m.Restaurant.DeliveryFeeMin == null ||
                m.Restaurant.DeliveryFeeMin == 0
                    ? "Free"
                    : $"₦{m.Restaurant.DeliveryFeeMin:N0}",
                m.Restaurant.EstDeliveryMin != null
                    ? $"{m.Restaurant.EstDeliveryMin}–{m.Restaurant.EstDeliveryMax} min"
                    : "~30 min"))
             .ToListAsync(cancellationToken);
    }
}
