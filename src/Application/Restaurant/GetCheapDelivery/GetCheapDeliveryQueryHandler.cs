using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetCheapDelivery;

internal sealed class GetCheapDeliveryQueryHandler(IApplicationDbContext db) : IQueryHandler<GetCheapDeliveryQuery, IReadOnlyList<HomeMenuItemDto>>
{
    public async Task<Result<IReadOnlyList<HomeMenuItemDto>>> Handle(GetCheapDeliveryQuery request, CancellationToken cancellationToken)
    {
        var data = await db.MenuItem
            .AsNoTracking()
            .Where(m =>
                m.IsAvailable &&
                m.Restaurant.IsActive &&
                m.Restaurant.IsOpen &&
                (m.Restaurant.DeliveryFeeMin == null ||
                 m.Restaurant.DeliveryFeeMin < request.MaxDeliveryFee))
            .OrderBy(m => m.Restaurant.DeliveryFeeMin ?? 0)
            .ThenByDescending(m => m.Restaurant.Rating)
            .Take(request.Limit)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.ImageUrl,
                m.Price,
                m.DiscountPrice,
                m.PrepTimeMin,
                m.RestaurantId,
                m.Restaurant.DeliveryFeeMin,
                RestaurantName = m.Restaurant.Name,
                RestaurantLogo = m.Restaurant.LogoUrl,
                RestaurantRating = m.Restaurant.Rating,
                EstMin = m.Restaurant.EstDeliveryMin,
                EstMax = m.Restaurant.EstDeliveryMax

            })
            .ToListAsync(cancellationToken);

        return data.Select(m => new HomeMenuItemDto(
            m.Id,
            m.Name,
            m.ImageUrl,
            m.Price,
            m.DiscountPrice,
            m.PrepTimeMin,
            m.RestaurantId,
            m.RestaurantName,
            m.RestaurantLogo,
            m.RestaurantRating,
            m.DeliveryFeeMin is null or 0
                ? "Free"
                : $"₦{m.DeliveryFeeMin:N0}",
            m.EstMin != null
                ? $"{m.EstMin}–{m.EstMax} min"
                : "~30 min"
        )).ToList();
    }
}
