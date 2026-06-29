using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetBestChoice;

internal class GetBestChoiceQueryHandler(IApplicationDbContext db) : IQueryHandler<GetBestChoiceQuery, IReadOnlyList<HomeMenuItemDto>>
{
    public async Task<Result<IReadOnlyList<HomeMenuItemDto>>> Handle(GetBestChoiceQuery request, CancellationToken cancellationToken)
    {
        var data = await db.MenuItem
            .AsNoTracking()
            .Where(m =>
                m.IsAvailable &&
                m.Restaurant.IsActive &&
                m.Restaurant.IsOpen &&
                m.Restaurant.Rating > request.MinRating)
            .OrderByDescending(m => m.Restaurant.Rating)
            .ThenByDescending(m => m.IsPopular)
            .ThenByDescending(m => m.Restaurant.TotalReviews)
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
                RestaurantName = m.Restaurant.Name,
                RestaurantLogo = m.Restaurant.LogoUrl,
                RestaurantRating = m.Restaurant.Rating,
                m.Restaurant.DeliveryFeeMin,
                m.Restaurant.EstDeliveryMin,
                m.Restaurant.EstDeliveryMax,
                m.Description,
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
            m.EstDeliveryMin != null
                ? $"{m.EstDeliveryMin}–{m.EstDeliveryMax} min"
                : "~30 min", m.Description
        )).ToList();
    }
}
