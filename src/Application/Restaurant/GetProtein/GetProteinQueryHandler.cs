using Application.Abstractions.Data;
using Application.Abstractions.Helpers;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetProtein;
internal sealed class GetProteinFixQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetProteinFixQuery, IReadOnlyList<HomeSectionItemDto>>
{
    private static readonly string[] Keywords =
    [
        "suya", "grill", "grills", "grilled", "chicken", "barbecue",
        "bbq", "protein", "beef suya", "chicken suya", "ram suya",
        "peppered chicken", "fried chicken", "roasted chicken",
        "chicken laps", "beef", "kilishi", "assorted", "peri-peri",
        "chicken kickers", "quarter chicken", "half chicken",
        "full chicken", "laps", "smoked", "skewer"
    ];

    public async Task<Result<IReadOnlyList<HomeSectionItemDto>>> Handle(
        GetProteinFixQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.MenuItem.MenuItem> baseQuery = db.MenuItem
            .AsNoTracking()
            .Where(m =>
                m.IsAvailable &&
                m.Restaurant.IsActive &&
                m.Restaurant.IsOpen);

        var items = await KeywordPredicateBuilder
            .WhereMatchesAnyKeyword(baseQuery, Keywords)
            .OrderByDescending(m => m.IsPopular)
            .ThenByDescending(m => m.Restaurant.Rating)
            .ThenByDescending(m => m.Restaurant.TotalReviews)
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
