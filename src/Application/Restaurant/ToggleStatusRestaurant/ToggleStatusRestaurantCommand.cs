using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.ToggleStatusRestaurant;
public sealed record ToggleStatusRestaurantCommand(Guid RestaurantId, bool IsOpen) : ICommand<RestaurantDto>;
