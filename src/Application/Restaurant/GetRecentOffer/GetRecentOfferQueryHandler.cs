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
        var items = await db.MenuItem
         .AsNoTracking()
         .Where(m => m.IsAvailable &&
                     m.DiscountPrice != null &&
                     m.DiscountPrice > 0 &&
                     m.Restaurant.IsActive &&
                     m.Restaurant.IsOpen)
         .OrderByDescending(m => m.CreatedAt)
         .Take(query.Limit)
         .Select(m => new
         {
             m.Id,
             m.Name,
             m.ImageUrl,
             m.Price,
             m.DiscountPrice,
             m.PrepTimeMin,
             m.RestaurantId,
             RestaurantName = m.Restaurant.Name,
             m.Restaurant.LogoUrl,
             m.Restaurant.Rating,
             m.Restaurant.DeliveryFeeMin,
             m.Restaurant.EstDeliveryMin,
             m.Restaurant.EstDeliveryMax
         })
         .ToListAsync(cancellationToken);

        return items.Select(m => new HomeMenuItemDto(
            m.Id,
            m.Name,
            m.ImageUrl,
            m.Price,
            m.DiscountPrice,
            m.PrepTimeMin,
            m.RestaurantId,
            m.RestaurantName,
            m.LogoUrl,
            m.Rating,
            m.DeliveryFeeMin is null or 0
                ? "Free"
                : $"₦{m.DeliveryFeeMin:N0}",
            m.EstDeliveryMin != null
                ? $"{m.EstDeliveryMin}–{m.EstDeliveryMax} min"
                : "~30 min"
        )).ToList();
    }
}
