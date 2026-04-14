using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using SharedKernel;

namespace Application.Restaurant.GetNearbyRestaurant;

public sealed record GetNearbyRestaurantQuery(
    double lat, double lng, double RadiusKm = 5, int PageSize = 1000,
    int pageNumber = 1) : IQuery<PaginatedResult<RestaurantDto>>;
