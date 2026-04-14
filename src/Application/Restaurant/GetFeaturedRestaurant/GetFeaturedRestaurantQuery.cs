using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using SharedKernel;

namespace Application.Restaurant.GetFeaturedRestaurant;

public sealed record GetFeaturedRestaurantQuery() : IQuery<PaginatedResult<RestaurantDto>>;
