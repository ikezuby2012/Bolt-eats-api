using Application.Abstractions.Messaging;

namespace Application.Restaurant.DeleteRestaurant;

public sealed record DeleteRestaurantCommand(Guid Id) : ICommand;
