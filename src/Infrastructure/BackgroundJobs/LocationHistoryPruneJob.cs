using Application.Abstractions.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.BackgroundJobs;

public sealed class LocationHistoryPruneJob(IApplicationDbContext db)
{
    private static readonly TimeSpan HotRetention = TimeSpan.FromHours(24);

    private static readonly TimeSpan ColdStorageRetention = TimeSpan.FromDays(90);

    [AutomaticRetry(Attempts = 2)]
    public async Task PruneAsync(CancellationToken cancellationToken)
    {
        DateTime utcNow = DateTime.UtcNow;

        DateTime archiveCutoff = utcNow - HotRetention;
        DateTime permanentDeleteCutoff = utcNow - ColdStorageRetention;

        await db.RiderLocations
           .Where(r =>
               !r.IsSoftDeleted &&
               r.CreatedAt < archiveCutoff)
           .ExecuteUpdateAsync(setters =>
               setters
                   .SetProperty(
                       r => r.IsSoftDeleted,
                       true)
                   .SetProperty(
                       r => r.UpdatedAt,
                       utcNow),
               cancellationToken);

        /// =========================================
        /// PERMANENT DELETE
        /// 90d+
        /// =========================================
        await db.RiderLocations
            .Where(r =>
                r.CreatedAt < permanentDeleteCutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
