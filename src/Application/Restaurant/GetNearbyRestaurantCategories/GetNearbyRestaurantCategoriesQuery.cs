using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using SharedKernel;

namespace Application.Restaurant.GetNearbyRestaurantCategories;

public sealed record GetNearbyRestaurantCategoriesQuery(
    double Lat,
    double Lng,
    double RadiusKm = 5,
    int PageNumber = 1,
    int PageSize = 20)
    : IQuery<PaginatedResult<NearbyCategoryDto>>;
