using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetRestaurantMenu;

internal sealed class GetRestaurantMenuQueryHandler(IApplicationDbContext context) : IQueryHandler<GetRestaurantMenuQuery, IEnumerable<MenuItemDto>>
{
    public async Task<Result<IEnumerable<MenuItemDto>>> Handle(GetRestaurantMenuQuery query, CancellationToken cancellationToken)
    {
        bool restaurantExist = await context.Restaurants.AnyAsync(x => x.Id == query.RestaurantId, cancellationToken);

        if (!restaurantExist)
        {
            return Result.Failure<IEnumerable<MenuItemDto>>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not found!"));
        }

        List<MenuItemDto> menuItems = await context.MenuItem.AsNoTracking().Where(c => c.RestaurantId == query.RestaurantId).Include(x => x.Category).Select(x => (MenuItemDto)x).ToListAsync(cancellationToken);

        return menuItems;
    }
}
