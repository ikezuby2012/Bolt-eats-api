using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetRestaurantById;

public sealed record GetRestaurantByIdQuery(Guid Id) : IQuery<RestaurantDto>;
