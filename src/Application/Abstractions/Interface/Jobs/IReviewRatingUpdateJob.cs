namespace Application.Abstractions.Interface.Jobs;

public interface IReviewRatingUpdateJob
{
    Task RecomputeAsync(Guid restaurantId, CancellationToken cancellationToken);
}
