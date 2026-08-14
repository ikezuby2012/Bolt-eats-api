using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetRelatedMenuItemsByCategory;
internal sealed class GetRelatedMenuItemsByCategoryQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetRelatedMenuItemsByCategoryQuery, IReadOnlyList<RelatedMenuItemDto>>
{
    public async Task<Result<IReadOnlyList<RelatedMenuItemDto>>> Handle(
        GetRelatedMenuItemsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.MenuItem.MenuItem> query = db.MenuItem
            .AsNoTracking()
            .Where(m => m.IsAvailable &&
                        m.Restaurant.IsActive &&
                        m.Restaurant.IsOpen &&
                        m.Id != request.ExcludeMenuItemId &&
                        EF.Functions.Like(m.Category.Name, $"%{request.CategoryName}%"));

        // Optionally surface items from other restaurants for discovery
        if (request.ExcludeRestaurantId.HasValue)
        {
            query = query.Where(m => m.RestaurantId != request.ExcludeRestaurantId.Value);
        }


        List<RelatedMenuItemDto> items = await query
            .OrderByDescending(m => m.IsPopular)
            .ThenByDescending(m => m.Restaurant.Rating)
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

        return Result.Success<IReadOnlyList<RelatedMenuItemDto>>(items);
    }
}
