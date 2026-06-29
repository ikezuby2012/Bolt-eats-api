using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Domain.MenuItem;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.AddMenuItem;

internal sealed class AddMenuItemCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<AddMenuItemCommand, MenuItemDto>
{
    public async Task<Result<MenuItemDto>> Handle(AddMenuItemCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Restaurant.Restaurant? restaurant = await context.Restaurants.FirstOrDefaultAsync(x => x.Id == command.RestaurantId && x.IsActive, cancellationToken);

        if (restaurant == null)
        {
            return Result.Failure<MenuItemDto>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not found!"));
        }
        if (restaurant.OwnerId != userId)
        {
            return Result.Failure<MenuItemDto>(Domain.Common.CommonErrors.CustomErrorMessage("You do not have permission to modify this restaurant!"));
        }

        bool catergoryExists = await context.Category.AnyAsync(c => c.Id == command.CategoryId && c.RestaurantId == command.RestaurantId, cancellationToken);

        if (!catergoryExists)
        {
            return Result.Failure<MenuItemDto>(Domain.Common.CommonErrors.CustomErrorMessage("Category not found!"));
        }

        var newItem = new MenuItem
        {
            RestaurantId = command.RestaurantId,
            CategoryId = command.CategoryId,
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            DiscountPrice = command.DiscountPrice,
            Calories = command.Calories,
            PrepTimeMin = command.PrepTimeMin,
            IsAvailable = command.IsAvailable,
            IsPopular = command.IsPopular,
            SortOrder = command.SortOrder,
            ImageUrl = command.ImageLink,
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = userId.ToString(),
        };

        await context.MenuItem.AddAsync(newItem, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success((MenuItemDto)newItem);
    }
}
