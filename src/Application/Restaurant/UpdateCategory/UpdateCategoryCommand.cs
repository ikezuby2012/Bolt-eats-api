using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.UpdateCategory;

public sealed record UpdateCategoryCommand(Guid RestaurantId, Guid CategoryId, string Name, int SortOrder) : ICommand<CategoryDto>;

