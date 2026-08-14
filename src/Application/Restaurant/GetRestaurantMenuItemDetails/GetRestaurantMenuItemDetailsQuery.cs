using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetRestaurantMenuItemDetails;

public sealed record GetRestaurantMenuItemDetailsQuery(Guid MenuItemId) : IQuery<MenuItemDetailDto>;
