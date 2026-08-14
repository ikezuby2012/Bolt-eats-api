using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Application.Users.Dto;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using SharedKernel;

namespace Application.Restaurant.GetNearbyRestaurant;

internal sealed class GetNearbyRestaurantQueryHandler(IApplicationDbContext context) : IQueryHandler<GetNearbyRestaurantQuery, PaginatedResult<RestaurantDto>>
{
    private static readonly GeometryFactory _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
    public async Task<Result<PaginatedResult<RestaurantDto>>> Handle(GetNearbyRestaurantQuery request, CancellationToken cancellationToken)
    {
        Point searchOrigin = _geometryFactory.CreatePoint(
             new Coordinate(request.lng, request.lat)
         );

        double radius = request.RadiusKm * 1000;

        IQueryable<Domain.Address.Address> query = context.Addresses.AsNoTracking().AsQueryable().Where(r => r.RestaurantId != null && r.Location != null && r.Location.IsWithinDistance(searchOrigin, radius));
        int totalItems = await query.CountAsync(cancellationToken);

        List<RestaurantDto> allRestaurants = await query.OrderBy(a => a.Location!.Distance(searchOrigin))
            .Skip((request.pageNumber - 1) * request.PageSize)
            .Select(a => new RestaurantDto
            {
                Id = a.Restaurant!.Id,
                OwnerId = a.Restaurant.OwnerId,
                Name = a.Restaurant.Name,
                Description = a.Restaurant.Description,
                LogoUrl = a.Restaurant.LogoUrl,
                BannerUrl = a.Restaurant.BannerUrl,
                PhoneNumber = a.Restaurant.PhoneNumber,
                Email = a.Restaurant.Email,
                Rating = a.Restaurant.Rating,
                TotalReviews = a.Restaurant.TotalReviews,
                DeliveryFeeMin = a.Restaurant.DeliveryFeeMin,
                DeliveryFeeMax = a.Restaurant.DeliveryFeeMax,
                MinOrderAmount = a.Restaurant.MinOrderAmount,
                EstDeliveryMin = a.Restaurant.EstDeliveryMin,
                EstDeliveryMax = a.Restaurant.EstDeliveryMax,
                IsOpen = a.Restaurant.IsOpen,
                IsActive = a.Restaurant.IsActive,
                IsCompanyPartner = a.Restaurant.CompanyPartner,
                CreatedAt = a.Restaurant.CreatedAt,
                UpdatedAt = a.Restaurant.UpdatedAt,
                CreatedBy = a.Restaurant.CreatedBy,
                UpdatedBy = a.Restaurant.UpdatedBy,
                Address = (AddressDto)a
            }).ToListAsync(cancellationToken);


        return new PaginatedResult<RestaurantDto>
        {
            Data = allRestaurants,
            TotalItems = totalItems,
            PageSize = request.PageSize,
            PageNumber = request.pageNumber,
        };
    }
}
