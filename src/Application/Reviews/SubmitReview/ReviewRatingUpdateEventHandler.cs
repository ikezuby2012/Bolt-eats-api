using Application.Abstractions.Interface.Jobs;
using Application.Abstractions.Services;
using Domain.Review;
using SharedKernel;

namespace Application.Reviews.SubmitReview;

internal sealed class ReviewRatingUpdateEventHandler(IBackgroundJobClient jobClient) : IDomainEventHandler<ReviewRatingUpdateEvent>
{
    public Task Handle(ReviewRatingUpdateEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid restaurantId = domainEvent.RestaurantId;

        jobClient.Enqueue<IReviewRatingUpdateJob>(j => j.RecomputeAsync(restaurantId, cancellationToken));

        return Task.CompletedTask;
    }
}
