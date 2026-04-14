using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetAllRestaurant;

internal sealed class GetAllRestaurantQueryHandler(IApplicationDbContext context) : IQueryHandler<GetAllRestaurantQuery, PaginatedResult<RestaurantDto>>
{
    public async Task<Result<PaginatedResult<RestaurantDto>>> Handle(GetAllRestaurantQuery req, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Restaurant.Restaurant> query = context.Restaurants.AsNoTracking().AsQueryable().Include(x => x.Addresses).Where(x => (!req.IsActive.HasValue || x.IsActive == req.IsActive.Value)
                            && (!req.DateFrom.HasValue || x.CreatedAt >= req.DateFrom.Value)
                            && (!req.DateTo.HasValue || x.CreatedAt <= req.DateTo.Value));

        int totalItems = await query.CountAsync(cancellationToken);

        List<RestaurantDto> allRestaurants = await query.OrderByDescending(m => m.CreatedAt)
            .Skip((req.pageNumber - 1) * req.PageSize)
            .Select(x => (RestaurantDto)x).ToListAsync(cancellationToken);

        return new PaginatedResult<RestaurantDto>
        {
            Data = allRestaurants,
            TotalItems = totalItems,
            PageSize = req.PageSize,
            PageNumber = req.pageNumber,
        };
    }
}
