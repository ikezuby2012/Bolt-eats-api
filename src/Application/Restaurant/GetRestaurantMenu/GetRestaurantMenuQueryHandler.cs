using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetRestaurantMenu;

internal sealed class GetRestaurantMenuQueryHandler(IApplicationDbContext context) : IQueryHandler<GetRestaurantMenuQuery, IEnumerable<CategoryDto>>
{
    public async Task<Result<IEnumerable<CategoryDto>>> Handle(GetRestaurantMenuQuery query, CancellationToken cancellationToken)
    {
        bool restaurantExist = await context.Restaurants.AnyAsync(x => x.Id == query.RestaurantId, cancellationToken);

        if (restaurantExist)
        {
            return Result.Failure<IEnumerable<CategoryDto>>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not found!"));
        }

        List<CategoryDto> menuCategories = await context.Category.AsNoTracking().Where(c => c.RestaurantId == query.RestaurantId).OrderBy(c => c.DisplayOrder).Select(x => (CategoryDto)x).ToListAsync(cancellationToken);

        return menuCategories;
    }
}
