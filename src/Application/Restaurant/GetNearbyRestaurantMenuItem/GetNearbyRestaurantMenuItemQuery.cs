using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using SharedKernel;

namespace Application.Restaurant.GetNearbyRestaurantMenuItem;

public sealed record GetNearbyRestaurantMenuItemsQuery(
    double Lat,
    double Lng,
    double RadiusKm = 5,
    Guid? CategoryId = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20)
: IQuery<PaginatedResult<NearbyMenuItemDto>>;
