using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetQuickEats;
internal class GetQuickEatsQueryHandler(IApplicationDbContext db) : IQueryHandler<GetQuickEatsQuery, IReadOnlyList<HomeMenuItemDto>>
{
    public async Task<Result<IReadOnlyList<HomeMenuItemDto>>> Handle(GetQuickEatsQuery query, CancellationToken cancellationToken)
    {
        var items = await db.MenuItem
            .AsNoTracking()
            .Where(m => m.IsAvailable &&
                        m.Restaurant.IsActive &&
                        m.Restaurant.IsOpen &&
                        m.PrepTimeMin > 0)
            .OrderBy(m => m.PrepTimeMin)
            .ThenByDescending(m => m.Restaurant.Rating)
            .Take(query.limit)
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
                RestaurantLogo = m.Restaurant.LogoUrl,
                RestaurantRating = m.Restaurant.Rating,
                m.Restaurant.DeliveryFeeMin
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
            m.RestaurantLogo,
            m.RestaurantRating,
            m.DeliveryFeeMin == null || m.DeliveryFeeMin == 0
                ? "Free"
                : $"₦{m.DeliveryFeeMin:N0}",
            $"{m.PrepTimeMin} min prep"))
        .ToList();
    }
}
