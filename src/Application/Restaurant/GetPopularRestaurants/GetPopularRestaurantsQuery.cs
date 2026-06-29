using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetPopularRestaurants;

public sealed record GetPopularRestaurantsQuery(
    double MinRating = 4,
    int Limit = 10)
    : IQuery<IReadOnlyList<RestaurantDto>>;
