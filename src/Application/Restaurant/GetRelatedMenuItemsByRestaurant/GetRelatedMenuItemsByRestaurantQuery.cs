using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;

namespace Application.Restaurant.GetRelatedMenuItemsByRestaurant;

public sealed record GetRelatedMenuItemsByRestaurantQuery(Guid RestaurantId, Guid ExcludeMenuItemId, int Limit = 10) : IQuery<IReadOnlyList<RelatedMenuItemDto>>;
