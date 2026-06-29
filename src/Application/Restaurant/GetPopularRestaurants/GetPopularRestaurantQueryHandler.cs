using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetPopularRestaurants;

internal sealed class GetPopularRestaurantsQueryHandler(IApplicationDbContext db) : IQueryHandler<GetPopularRestaurantsQuery, IReadOnlyList<RestaurantDto>>
{
    public async Task<Result<IReadOnlyList<RestaurantDto>>> Handle(
        GetPopularRestaurantsQuery request,
        CancellationToken cancellationToken)
    {
        return await db.Restaurants
            .AsNoTracking()
            .Where(r => r.IsActive &&
                        r.IsOpen &&
                        r.Rating >= request.MinRating)
            .OrderByDescending(r => r.Rating)
            .ThenByDescending(r => r.TotalReviews)
            .Take(request.Limit)
            .Select(r => (RestaurantDto)r)
            .ToListAsync(cancellationToken);
    }
}
