using Application.Abstractions.Messaging;
using Application.Restaurant.CreateRestaurant;
using Application.Restaurant.Dto;

namespace Application.Restaurant.UpdateRestaurantInfo;

public sealed record UpdateRestaurantCommand(
    Guid Id,
    string Name,
    string Description,
    string PhoneNumber,
    string? Email,
    string? LogoLink,
    string? BannerLink,
    decimal? DeliveryFeeMin,
    decimal? DeliveryFeeMax,
    decimal? MinOrderAmount,
    int? EstDeliveryMin,
    int? EstDeliveryMax,
    bool CompanyPartner,
    CreateAddressRequest Address
    ) : ICommand<RestaurantDto>;
