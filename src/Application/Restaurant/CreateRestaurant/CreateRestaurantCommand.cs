using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.CreateRestaurant;

public sealed record CreateAddressRequest(
    string Street,
    string City,
    string State,
    string Country,
    decimal Lat,
    decimal Lng, 
    string? PostalCode);
public sealed record CreateRestaurantCommand(
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
