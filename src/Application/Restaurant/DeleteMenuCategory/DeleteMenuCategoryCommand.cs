

using Application.Abstractions.Messaging;

namespace Application.Restaurant.DeleteMenuCategory;

public sealed record DeleteMenuCategoryCommand(Guid CategoryId) : ICommand;
