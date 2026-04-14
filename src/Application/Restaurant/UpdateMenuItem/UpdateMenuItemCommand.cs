using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.UpdateMenuItem;
public sealed record UpdateMenuItemCommand(
    Guid MenuItemId,
    Guid RestaurantId,
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    string? ImageLink,
    int? Calories,
    int PrepTimeMin,
    bool IsAvailable,
    bool IsPopular,
    int SortOrder
    ) : ICommand<MenuItemDto>;
