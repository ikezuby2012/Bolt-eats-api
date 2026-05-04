using SharedKernel;

namespace Domain.Review;

public sealed record ReviewRatingUpdateEvent(Guid RestaurantId) : IDomainEvent;
