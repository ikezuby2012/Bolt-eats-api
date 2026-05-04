namespace Application.Abstractions.Services;

public interface IRatingCalculator
{
    Task<(double NewRating, int TotalReviews)> ComputeAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}
