using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetRelatedMenuItemsByCategory;

public sealed record GetRelatedMenuItemsByCategoryQuery(string CategoryName, Guid ExcludeMenuItemId, Guid? ExcludeRestaurantId, int Limit = 10) : IQuery<IReadOnlyList<RelatedMenuItemDto>>;
