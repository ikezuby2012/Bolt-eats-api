using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.UpdateCategory;

internal sealed class UpdateCategoryCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
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

        Domain.Category.Category? category = await context.Category.FirstOrDefaultAsync(x => x.Id == command.CategoryId && x.RestaurantId == command.RestaurantId, cancellationToken);

        if (category is null)
        {
            return Result.Failure<CategoryDto>(Domain.Common.CommonErrors.CustomErrorMessage("Category Not found!"));
        }

        category.Name = command.Name;
        category.DisplayOrder = command.SortOrder;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success((CategoryDto)category);
    }
}
