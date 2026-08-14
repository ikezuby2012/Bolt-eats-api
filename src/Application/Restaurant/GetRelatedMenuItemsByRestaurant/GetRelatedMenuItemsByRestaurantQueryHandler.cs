using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetRelatedMenuItemsByRestaurant;

internal sealed class GetRelatedMenuItemsByRestaurantQueryHandler(IApplicationDbContext db) : IQueryHandler<GetRelatedMenuItemsByRestaurantQuery, IReadOnlyList<RelatedMenuItemDto>>
{
    public async Task<Result<IReadOnlyList<RelatedMenuItemDto>>> Handle(GetRelatedMenuItemsByRestaurantQuery request, CancellationToken cancellationToken)
    {
        bool restaurantExists = await db.Restaurants.AnyAsync(r => r.Id == request.RestaurantId && r.IsActive, cancellationToken);

        if (!restaurantExists)
        {
            return Result.Failure<IReadOnlyList<RelatedMenuItemDto>>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not found!"));
        }

        List<RelatedMenuItemDto> items = await db.MenuItem
            .AsNoTracking()
            .Where(m => m.RestaurantId == request.RestaurantId &&
                        m.Id != request.ExcludeMenuItemId &&
                        m.IsAvailable)
            .OrderByDescending(m => m.IsPopular)
            .ThenBy(m => m.SortOrder)
            .Take(request.Limit)
            .Select(m => new RelatedMenuItemDto(
                m.Id,
                m.Name,
                m.Description,
                m.Price,
                m.DiscountPrice,
                m.ImageUrl,
                m.PrepTimeMin,
                m.IsPopular,
                m.Category.Name))
            .ToListAsync(cancellationToken);

        return items;
    }
}
