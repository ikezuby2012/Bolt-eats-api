using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetRestaurantMenuCategories;

public sealed record GetRestaurantMenuCategoriesQuery(Guid RestaurantId) : IQuery<IReadOnlyList<MenuCategoryDto>>;
