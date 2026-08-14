using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.GetRestaurantMenuItemDetails;

internal sealed class GetRestaurantMenuItemDetailsQueryHandler(IApplicationDbContext db) : IQueryHandler<GetRestaurantMenuItemDetailsQuery, MenuItemDetailDto>
{
    public async Task<Result<MenuItemDetailDto>> Handle(GetRestaurantMenuItemDetailsQuery query, CancellationToken cancellationToken)
    {
        MenuItemDetailDto? item = await db.MenuItem
            .AsNoTracking()
            .Where(m => m.Id == query.MenuItemId && m.IsAvailable)
            .Select(m => new MenuItemDetailDto(
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
                m.SortOrder,
                m.CategoryId,
                m.Category.Name,
                m.RestaurantId,
                m.Restaurant.Name,
                m.Restaurant.LogoUrl,
                m.Restaurant.BannerUrl,
                m.Restaurant.Rating,
                m.Restaurant.TotalReviews,
                m.Restaurant.IsOpen,
                m.Restaurant.DeliveryFeeMin == null || m.Restaurant.DeliveryFeeMin == 0
                    ? "Free"
                    : $"₦{m.Restaurant.DeliveryFeeMin:N0}",
                m.Restaurant.EstDeliveryMin != null
                    ? $"{m.Restaurant.EstDeliveryMin}–{m.Restaurant.EstDeliveryMax} min"
                    : "~30 min",
                m.Restaurant.MinOrderAmount))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return Result.Failure<MenuItemDetailDto>(Domain.Common.CommonErrors.CustomErrorMessage("Menu Item Not found!"));
        }

        return item;
    }
}
