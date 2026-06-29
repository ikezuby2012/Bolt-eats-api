using System.Globalization;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Domain.Address;
using Serilog;
using SharedKernel;

namespace Application.Restaurant.CreateRestaurant;

public sealed class CreateRestaurantCommandHandler(IDateTimeProvider dateTimeProvider, IApplicationDbContext context, IUserContext userContext) : ICommandHandler<CreateRestaurantCommand, RestaurantDto>
{
    public async Task<Result<RestaurantDto>> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        var restaurant = new Domain.Restaurant.Restaurant
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            Name = request.Name,
            Description = request.Description,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            LogoUrl = request.LogoLink,
            BannerUrl = request.BannerLink,
            DeliveryFeeMin = request.DeliveryFeeMin,
            DeliveryFeeMax = request.DeliveryFeeMax,
            MinOrderAmount = request.MinOrderAmount,
            EstDeliveryMin = request.EstDeliveryMin,
            EstDeliveryMax = request.EstDeliveryMax,
            CompanyPartner = request.CompanyPartner,
            IsOpen = false,
            IsActive = true,
            CreatedAt = dateTimeProvider.UtcNow,
            CreatedBy = userId.ToString(),
            Rating = request.Rating ?? 0,
            Addresses = new List<Address>
            {
                new Address
                 {
                    Street = request.Address.Street,
                    City = request.Address.City,
                    State = request.Address.State,
                    Country = request.Address.Country,
                    CreatedAt = dateTimeProvider.UtcNow,
                    CreatedBy = userId.ToString(),
                    LongitudeRaw = request.Address.Lng.ToString(CultureInfo.InvariantCulture),
                    Longitude = request.Address.Lng,
                    LatitudeRaw = request.Address.Lat.ToString(CultureInfo.InvariantCulture),
                    Latitude = request.Address.Lat,
                    PostalCode = request.Address.PostalCode ?? "",
                    Location = Address.CreatePoint((double)request.Address.Lat, (double)request.Address.Lng),
                    Label = $"{request.Name} address",
                }
            }
        };

        await context.Restaurants.AddAsync(restaurant, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return (RestaurantDto)restaurant;
    }
}
