using Application.Abstractions.Data;
using Application.Abstractions.Interface.Jobs;
using Application.Abstractions.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.BackgroundJobs;

[AutomaticRetry(Attempts = 3, DelaysInSeconds = [10, 30, 60])]
internal sealed class ReviewRatingUpdateJob(IApplicationDbContext context, IRatingCalculator ratingCalculator) : IReviewRatingUpdateJob
{
    public async Task RecomputeAsync(Guid restaurantId, CancellationToken cancellationToken)
    {
        Domain.Restaurant.Restaurant? restaurant = await context.Restaurants.FirstOrDefaultAsync(r => r.Id == restaurantId, cancellationToken);

        if (restaurant is null)
        {
            return;
        }

        (double newRating, int totalReviews) = await ratingCalculator.ComputeAsync(
            restaurantId, cancellationToken);

        restaurant.Rating = newRating;
        restaurant.TotalReviews = totalReviews;

        await context.SaveChangesAsync(cancellationToken);
    }
}
