using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using SharedKernel;

namespace Application.Restaurant.GetNearbyRestaurantMenuItem;

#pragma warning disable CA1304 // Specify CultureInfo
#pragma warning disable CA1311 // Specify a culture or use an invariant version

internal sealed class GetNearbyRestaurantMenuItemsQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetNearbyRestaurantMenuItemsQuery, PaginatedResult<NearbyMenuItemDto>>
{
    private static readonly GeometryFactory _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public async Task<Result<PaginatedResult<NearbyMenuItemDto>>> Handle(
        GetNearbyRestaurantMenuItemsQuery request,
        CancellationToken cancellationToken)
    {
        Point searchOrigin = _geometryFactory.CreatePoint(
            new Coordinate(request.Lng, request.Lat));
        double radius = request.RadiusKm * 1000;

        List<Guid> nearbyRestaurantIds = await context.Addresses
            .AsNoTracking()
            .Where(a => a.RestaurantId != null &&
                        a.Location != null &&
                        a.Location.IsWithinDistance(searchOrigin, radius) &&
                        a.Restaurant!.IsActive &&
                        a.Restaurant.IsOpen)
            .Select(a => a.RestaurantId!.Value)
            .ToListAsync(cancellationToken);

        if (!nearbyRestaurantIds.Any())
        {
            return new PaginatedResult<NearbyMenuItemDto>
            {
                Data = [],
                TotalItems = 0,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber,
            };
        }


        IQueryable<Domain.MenuItem.MenuItem> baseQuery = context.MenuItem
            .AsNoTracking()
            .Where(m => m.IsAvailable &&
                        nearbyRestaurantIds.Contains(m.RestaurantId));

        // Optional category filter
        if (request.CategoryId.HasValue)
        {
            baseQuery = baseQuery.Where(m => m.CategoryId == request.CategoryId.Value);
        }

        // Optional search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            baseQuery = baseQuery.Where(m =>
                EF.Functions.Like(m.Name.ToLower(), $"%{request.Search.ToLower()}%") ||
                EF.Functions.Like(m.Description.ToLower(), $"%{request.Search.ToLower()}%"));
        }

        int total = await baseQuery.CountAsync(cancellationToken);

        List<NearbyMenuItemDto> items = await baseQuery
            .OrderByDescending(m => m.IsPopular)
            .ThenByDescending(m => m.Restaurant.Rating)
            .ThenBy(m => m.Price)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new NearbyMenuItemDto(
                m.Id,
                m.Name,
                m.Description,
                m.Price,
                m.DiscountPrice,
                m.ImageUrl,
                m.PrepTimeMin,
                m.IsPopular,
                m.Calories,
                m.CategoryId,
                m.Category.Name,
                m.RestaurantId,
                m.Restaurant.Name,
                m.Restaurant.LogoUrl,
                m.Restaurant.Rating,
                m.Restaurant.IsOpen,
                m.Restaurant.DeliveryFeeMin == null || m.Restaurant.DeliveryFeeMin == 0
                    ? "Free"
                    : $"₦{m.Restaurant.DeliveryFeeMin:N0}",
                m.Restaurant.EstDeliveryMin != null
                    ? $"{m.Restaurant.EstDeliveryMin}–{m.Restaurant.EstDeliveryMax} min"
                    : "~30 min"))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<NearbyMenuItemDto>
        {
            Data = items,
            TotalItems = total,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
        };
    }
}
#pragma warning restore CA1311 // Specify a culture or use an invariant version
#pragma warning restore CA1304 // Specify CultureInfo
