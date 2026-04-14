using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.UpdateRestaurantInfo;

internal sealed class UpdateRestaurantCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateRestaurantCommand, RestaurantDto>
{
    public async Task<Result<RestaurantDto>> Handle(UpdateRestaurantCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.Restaurant.Restaurant? restaurant = await context.Restaurants.FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (restaurant is null)
        {
            return Result.Failure<RestaurantDto>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant Not found!"));
        }

        if (restaurant.OwnerId != userId)
        {
            return Result.Failure<RestaurantDto>(Domain.Common.CommonErrors.CustomErrorMessage("You do not have permission to update this restaurant!"));
        }

        if (!string.IsNullOrEmpty(command.Name))
        {
            restaurant.Name = command.Name;
        }
        if (!string.IsNullOrEmpty(command.Description))
        {
            restaurant.Description = command.Description;
        }
        if (!string.IsNullOrEmpty(command.PhoneNumber))
        {
            restaurant.PhoneNumber = command.PhoneNumber;
        }
        if (!string.IsNullOrEmpty(command.Email))
        {
            restaurant.Email = command.Email;
        }
        if (!string.IsNullOrEmpty(command.LogoLink))
        {
            restaurant.LogoUrl = command.LogoLink;
        }
        if (!string.IsNullOrEmpty(command.BannerLink))
        {
            restaurant.BannerUrl = command.BannerLink;
        }
        if (command.DeliveryFeeMax.HasValue)
        {
            restaurant.DeliveryFeeMax = command.DeliveryFeeMax.Value;
        }
        if (command.DeliveryFeeMin.HasValue)
        {
            restaurant.DeliveryFeeMin = command.DeliveryFeeMin.Value;
        }
        if (command.MinOrderAmount.HasValue)
        {
            restaurant.MinOrderAmount = command.MinOrderAmount.Value;
        }
        if (command.EstDeliveryMax.HasValue)
        {
            restaurant.EstDeliveryMax = command.EstDeliveryMax.Value;
        }
        if (command.EstDeliveryMin.HasValue)
        {
            restaurant.EstDeliveryMin = command.EstDeliveryMin.Value;
        }
        restaurant.CompanyPartner = command.CompanyPartner;
        restaurant.UpdatedBy = userId.ToString();
        restaurant.UpdatedAt = dateTimeProvider.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return (RestaurantDto)restaurant;
    }
}
