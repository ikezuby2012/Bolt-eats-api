using Application.Abstractions.Data;
using Application.Abstractions.Helpers;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using SharedKernel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Restaurant.GetRiceDishes;

#pragma warning disable CA1304 // Specify CultureInfo
#pragma warning disable CA1311 // Specify a culture or use an invariant version
internal sealed class GetRiceDishesQueryHandler(IApplicationDbContext db) : IQueryHandler<GetRiceDishesQuery, IReadOnlyList<HomeSectionItemDto>>
{

    public async Task<Result<IReadOnlyList<HomeSectionItemDto>>> Handle(GetRiceDishesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.MenuItem.MenuItem> baseQuery = db.MenuItem
        .AsNoTracking()
        .Where(m =>
            m.IsAvailable &&
            m.Restaurant.IsActive &&
            m.Restaurant.IsOpen);


        IQueryable<Domain.MenuItem.MenuItem> filteredQuery = baseQuery.Where(m =>
            EF.Functions.Like(m.Name.ToLower(), "%rice%") ||
            EF.Functions.Like(m.Category.Name.ToLower(), "%grains%") ||
            EF.Functions.Like(m.Name.ToLower(), "%rice%") ||
            EF.Functions.Like(m.Category.Name.ToLower(), "%grains%"));

        var items = await filteredQuery
            .OrderByDescending(m => m.IsPopular)
            .ThenByDescending(m => m.Restaurant.Rating)
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

        return items.Select(m => new HomeSectionItemDto(
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
        )).ToList();
    }
}
#pragma warning restore CA1311 // Specify a culture or use an invariant version
#pragma warning restore CA1304 // Specify CultureInfo
