using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetFeaturedRestaurant;

internal sealed class GetFeaturedRestaurantQueryHandler(IApplicationDbContext context) : IQueryHandler<GetFeaturedRestaurantQuery, PaginatedResult<RestaurantDto>>
{
    public async Task<Result<PaginatedResult<RestaurantDto>>> Handle(GetFeaturedRestaurantQuery query, CancellationToken cancellationToken)
    {
        List<RestaurantDto> allRestaurants = await context.Restaurants.AsNoTracking()
            .Where(x => x.IsActive && x.IsOpen && x.Rating >= 4.5)
            .OrderByDescending(x => x.Rating)
            .Take(100)
            .Select(x => (RestaurantDto)x)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<RestaurantDto>
        {
            Data = allRestaurants,
            TotalItems = allRestaurants.Count,
            PageSize = 100,
            PageNumber = 1,
        };
    }
}
