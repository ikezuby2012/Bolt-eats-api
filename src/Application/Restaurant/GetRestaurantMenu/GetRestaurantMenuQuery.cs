using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetRestaurantMenu;

public sealed record GetRestaurantMenuQuery(Guid RestaurantId) : IQuery<IEnumerable<CategoryDto>>;
