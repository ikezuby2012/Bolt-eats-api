using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.DeleteRestaurant;
internal sealed class DeleteRestaurantCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<DeleteRestaurantCommand>
{
    public async Task<Result> Handle(DeleteRestaurantCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.Restaurant.Restaurant? restaurant = await context.Restaurants.FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (restaurant is null)
        {
            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not found!"));
        }

        restaurant.IsOpen = false;
        restaurant.IsActive = false;
        restaurant.IsSoftDeleted = false;

        restaurant.UpdatedBy = userId.ToString();
        restaurant.UpdatedAt = dateTimeProvider.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
