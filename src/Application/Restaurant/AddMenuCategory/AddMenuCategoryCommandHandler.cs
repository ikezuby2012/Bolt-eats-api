using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Domain.Category;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.AddMenuCategory;

internal sealed class AddMenuCategoryCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<AddMenuCategoryCommand, CategoryDto>
{
    public async Task<Result<CategoryDto>> Handle(AddMenuCategoryCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Restaurant.Restaurant? restaurant = await context.Restaurants.FirstOrDefaultAsync(x => x.Id == command.RestaurantId, cancellationToken);

        if (restaurant is null)
        {
            return Result.Failure<CategoryDto>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not found!"));
        }

        if (!restaurant.IsActive)
        {
            return Result.Failure<CategoryDto>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not Active!"));
        }
        if (restaurant.OwnerId != userId)
        {
            return Result.Failure<CategoryDto>(Domain.Common.CommonErrors.CustomErrorMessage("You do not have permission to modify this restaurant"));
        }

        var category = new Category
        {
            Name = command.Name,
            DisplayOrder = command.SortOrder,
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = userId.ToString(),
            RestaurantId = restaurant.Id,
            IsActive = true,
        };

        await context.Category.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return (CategoryDto)category;
    }
}
