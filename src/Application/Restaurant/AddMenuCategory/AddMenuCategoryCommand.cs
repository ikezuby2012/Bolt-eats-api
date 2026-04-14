using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.AddMenuCategory;

public sealed record AddMenuCategoryCommand(Guid RestaurantId, string Name, int SortOrder) : ICommand<CategoryDto>;
