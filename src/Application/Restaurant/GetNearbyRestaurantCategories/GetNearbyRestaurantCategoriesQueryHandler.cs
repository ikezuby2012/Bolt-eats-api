using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using SharedKernel;

namespace Application.Restaurant.GetNearbyRestaurantCategories;

internal sealed class GetNearbyRestaurantCategoriesQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetNearbyRestaurantCategoriesQuery, PaginatedResult<NearbyCategoryDto>>
{
    private static readonly GeometryFactory _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public async Task<Result<PaginatedResult<NearbyCategoryDto>>> Handle(
        GetNearbyRestaurantCategoriesQuery request,
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
                        a.Restaurant!.IsActive)
            .Select(a => a.RestaurantId!.Value)
            .ToListAsync(cancellationToken);

        if (!nearbyRestaurantIds.Any())
        {
            return new PaginatedResult<NearbyCategoryDto>
            {
                Data = [],
                TotalItems = 0,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber,
            };
        }


        IQueryable<Domain.Category.Category> baseQuery = context.Category
            .AsNoTracking()
            .Where(c => nearbyRestaurantIds.Contains(c.RestaurantId));

        int total = await baseQuery.CountAsync(cancellationToken);

        List<NearbyCategoryDto> items = await baseQuery
            .OrderBy(c => c.Restaurant!.Name)
            .ThenBy(c => c.DisplayOrder)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new NearbyCategoryDto(
                c.Id,
                c.Name,
                c.DisplayOrder ?? 0,
                c.RestaurantId,
                c.Restaurant!.Name,
                c.Restaurant.LogoUrl,
                c.Restaurant.Rating,
                c.Restaurant.IsOpen, 0))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<NearbyCategoryDto>
        {
            Data = items,
            TotalItems = total,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
        };
    }
}
