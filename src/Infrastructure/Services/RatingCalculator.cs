using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

internal sealed class RatingCalculator(IApplicationDbContext context) : IRatingCalculator
{
    public async Task<(double NewRating, int TotalReviews)> ComputeAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var stats = await context.Review
            .Where(r => r.RestaurantId == restaurantId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Average = g.Average(r => (double)r.Rating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats is null
            ? (0.0, 0)
            : (Math.Round(stats.Average, 1), stats.Count);
    }
}
