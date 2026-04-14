using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.UpdateMenuItem;
internal sealed class UpdateMenuItemCommandHandlerr(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateMenuItemCommand, MenuItemDto>
{
    public async Task<Result<MenuItemDto>> Handle(UpdateMenuItemCommand command, CancellationToken cancellationToken)
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

        Domain.MenuItem.MenuItem? menuItem = await context.MenuItem.FirstOrDefaultAsync(x => x.Id == command.MenuItemId, cancellationToken);

        if (menuItem == null)
        {
            return Result.Failure<MenuItemDto>(Domain.Common.CommonErrors.CustomErrorMessage("Menu Item does not exist!"));
        }

        menuItem.CategoryId = command.CategoryId;
        if (!string.IsNullOrEmpty(command.Name))
        {
            menuItem.Name = command.Name;
        }
        if (!string.IsNullOrEmpty(command.Description))
        {
            menuItem.Description = command.Description;
        }
        if (command.Price > 0)
        {
            menuItem.Price = command.Price;
        }
        if (command.DiscountPrice > 0)
        {
            menuItem.DiscountPrice = command.DiscountPrice;
        }
        if (command.Calories.HasValue && command.Calories > 0)
        {
            menuItem.Calories = command.Calories.Value;
        }
        if (command.PrepTimeMin > 0)
        {
            menuItem.PrepTimeMin = command.PrepTimeMin;
        }
        menuItem.IsAvailable = command.IsAvailable;
        menuItem.IsPopular = command.IsPopular;
        menuItem.UpdatedAt = dateTimeProvider.UtcNow;
        menuItem.UpdatedBy = userId.ToString();

        if (command.SortOrder > 0)
        {
            menuItem.SortOrder = command.SortOrder;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success((MenuItemDto)menuItem);
    }
}
