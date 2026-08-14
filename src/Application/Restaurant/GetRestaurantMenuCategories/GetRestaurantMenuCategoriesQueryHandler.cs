using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetRestaurantMenuCategories;

internal sealed class GetRestaurantMenuCategoriesQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetRestaurantMenuCategoriesQuery, IReadOnlyList<MenuCategoryDto>>
{
    public async Task<Result<IReadOnlyList<MenuCategoryDto>>> Handle(
        GetRestaurantMenuCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        bool restaurantExists = await db.Restaurants
            .AnyAsync(
                r => r.Id == request.RestaurantId && r.IsActive,
                cancellationToken);

        if (!restaurantExists)
        {
            return Result.Failure<IReadOnlyList<MenuCategoryDto>>(
               Error.NotFound("Restaurant.NotFound", "Restaurant not found."));
        }


        var menuItemsByCategory = await db.MenuItem
            .AsNoTracking()
            .Where(m => m.RestaurantId == request.RestaurantId && m.IsAvailable)
            .Include(m => m.Category) // needed for category name/order
            .OrderBy(m => m.Category.DisplayOrder ?? int.MaxValue)
            .ThenBy(m => m.SortOrder)
            .Select(m => new
            {
                m.CategoryId,
                m.Category.Name,
                m.Category.DisplayOrder,
                MenuItem = new MenuItemDto2(
                   m.Id,
                   m.Name,
                   m.Description,
                   m.Price,
                   m.DiscountPrice,
                   m.ImageUrl,
                        m.PrepTimeMin,
                        m.IsPopular,
                        m.IsAvailable,
                        m.Calories,
                        m.SortOrder)
            })
            .ToListAsync(cancellationToken);

        var categories = menuItemsByCategory
                    .GroupBy(x => new { x.CategoryId, x.Name, x.DisplayOrder })
                    .Select(g => new MenuCategoryDto(
                        g.Key.CategoryId,
                        g.Key.Name,
                        g.Key.DisplayOrder,
                        g.Select(x => x.MenuItem).ToList()))
                    .Where(c => c.Items.Count > 0) // filter empty categories
                    .ToList();


        return categories;
    }
}
