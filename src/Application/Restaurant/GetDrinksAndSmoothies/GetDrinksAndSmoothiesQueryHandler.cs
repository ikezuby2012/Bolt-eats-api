using Application.Abstractions.Data;
using Application.Abstractions.Helpers;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace Application.Restaurant.GetDrinksAndSmoothies;

internal sealed class GetDrinksAndSmoothiesQueryHandler(IApplicationDbContext db) : IQueryHandler<GetDrinksAndSmoothiesQuery, IReadOnlyList<HomeSectionItemDto>>
{
    private static readonly string[] Keywords =
    [
        "drink", "drinks", "smoothie", "juice", "cocktail", "mocktail",
        "milkshake", "zobo", "chapman", "smoothie bowl", "lemonade",
        "coffee", "tea", "malt", "soft drink", "water", "wine",
        "beer", "cold drink", "tiger nut", "kunu", "fura", "yoghurt",
        "frappe", "latte", "espresso", "flat white"
    ];

    public async Task<Result<IReadOnlyList<HomeSectionItemDto>>> Handle(
    GetDrinksAndSmoothiesQuery request,
    CancellationToken cancellationToken)
    {
        IQueryable<Domain.MenuItem.MenuItem> baseQuery = db.MenuItem
            .AsNoTracking()
            .Where(m =>
                m.IsAvailable &&
                m.Restaurant.IsActive &&
                m.Restaurant.IsOpen);

        IQueryable<Domain.MenuItem.MenuItem> filtered = KeywordPredicateBuilder
            .WhereMatchesAnyKeyword(baseQuery, Keywords);

        var items = await filtered
            .OrderByDescending(m => m.IsPopular)
            .ThenBy(m => m.Price) // cheapest drinks first
            .Take(request.Limit)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Price,
                m.DiscountPrice,
                m.ImageUrl,
                CategoryName = m.Category.Name,
                m.RestaurantId,
                RestaurantName = m.Restaurant.Name,
                RestaurantLogo = m.Restaurant.LogoUrl,
                RestaurantRating = m.Restaurant.Rating,
                m.Restaurant.DeliveryFeeMin,
                m.Restaurant.EstDeliveryMin,
                m.Restaurant.EstDeliveryMax,
                m.PrepTimeMin
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<HomeSectionItemDto>>(
            items.Select(m => new HomeSectionItemDto(
                m.Id,
                m.Name,
                m.Price,
                m.DiscountPrice,
                m.ImageUrl,
                m.CategoryName,
                m.RestaurantId,
                m.RestaurantName,
                m.RestaurantLogo,
                m.RestaurantRating,
                m.DeliveryFeeMin is null or 0
                    ? "Free"
                    : $"₦{m.DeliveryFeeMin:N0}",
                m.EstDeliveryMin != null
                    ? $"{m.EstDeliveryMin}–{m.EstDeliveryMax} min"
                    : "~30 min",
                m.PrepTimeMin
            )).ToList()
        );
    }
}
