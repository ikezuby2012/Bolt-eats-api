using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.ToggleStatusRestaurant;
internal sealed class ToggleStatusRestaurantCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<ToggleStatusRestaurantCommand, RestaurantDto>
{
    public async Task<Result<RestaurantDto>> Handle(ToggleStatusRestaurantCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Restaurant.Restaurant? restaurant = await context.Restaurants.FirstOrDefaultAsync(r => r.Id == command.RestaurantId && r.IsActive, cancellationToken);

        if (restaurant == null)
        {
            return Result.Failure<RestaurantDto>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not found!"));
        }
        if (restaurant.OwnerId != userId)
        {
            return Result.Failure<RestaurantDto>(Domain.Common.CommonErrors.CustomErrorMessage("You do not have permission to update this restaurant."));
        }
        restaurant.IsOpen = command.IsOpen;
        restaurant.UpdatedBy = userId.ToString();
        restaurant.UpdatedAt = dateTimeProvider.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return (RestaurantDto)restaurant;
    }
}
