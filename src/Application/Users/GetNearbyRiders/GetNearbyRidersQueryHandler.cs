using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Rider;
using Application.Tracking.Dto;
using Application.Users.Dto;
using Domain.Rider;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetNearbyRiders;

internal sealed class GetNearbyRidersQueryHandler(
    IApplicationDbContext db,
    IRiderLocationCache locationCache)
    : IQueryHandler<GetNearbyRidersQuery, IReadOnlyList<NearbyRiderDto>>
{
    public async Task<Result<IReadOnlyList<NearbyRiderDto>>> Handle(
        GetNearbyRidersQuery request,
        CancellationToken cancellationToken)
    {
        // ── 1. Query Redis GEO — sub-millisecond spatial lookup ───────────
        IReadOnlyList<NearbyRider> nearbyRiders = await locationCache.GetNearbyRidersAsync(
            latitude: request.Lat,
            longitude: request.Lng,
            radiusKm: request.RadiusKm,
            maxResults: request.Limit * 3,   // over-fetch then filter by status
            cancellationToken: cancellationToken);

        if (!nearbyRiders.Any())
        { return Result.Success<IReadOnlyList<NearbyRiderDto>>([]); }


        // ── 2. Filter to available riders only ────────────────────────────
        var availableRiders = new List<(NearbyRider Rider, RiderMeta? Meta)>();

        foreach (NearbyRider rider in nearbyRiders)
        {
            RiderAvailability? status = await locationCache.GetStatusAsync(
                rider.RiderId, cancellationToken);

            // null = TTL expired = offline — skip
            if (status != RiderAvailability.Available &&
                status != RiderAvailability.Busy)
            { continue; }

            RiderMeta? meta = await locationCache.GetMetaAsync(
                rider.RiderId, cancellationToken);

            availableRiders.Add((rider, meta));

            if (availableRiders.Count == request.Limit)
            { break; }

        }

        if (!availableRiders.Any())
        { return Result.Success<IReadOnlyList<NearbyRiderDto>>([]); }


        // ── 3. Fetch rider profile + user data from DB ────────────────────
        var riderIds = availableRiders
            .Select(r => r.Rider.RiderId)
            .ToList();

        var profiles = await db.RiderProfiles
            .AsNoTracking()
            .Where(p => riderIds.Contains(p.UserId))
            .Select(p => new
            {
                p.UserId,
                p.VehicleType,
                p.NumberPlate,
                p.VehicleColor,
                p.VehicleMake,
                p.VehicleModel,
                statusName = RiderVerificationStatus.FromValue(p.StatusId)!.Name
            })
            .ToListAsync(cancellationToken);

        var users = await db.Users
            .AsNoTracking()
            .Where(u => riderIds.Contains(u.Id))
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.ProfileImageUrl,
                u.IsOnline
            })
            .ToListAsync(cancellationToken);

        // ── 4. Join and project ───────────────────────────────────────────
        var result = availableRiders
            .Select(r =>
            {
                var user = users.FirstOrDefault(u => u.Id == r.Rider.RiderId);
                var profile = profiles.FirstOrDefault(p => p.UserId == r.Rider.RiderId);

                // Only include verified riders
                if (profile?.statusName != RiderVerificationStatus.Verified.Name)
                { return null; }

                return new NearbyRiderDto(
                    RiderId: r.Rider.RiderId,
                    FullName: user?.FirstName ?? "Rider",
                    ProfileImgLink: user?.ProfileImageUrl,
                    Latitude: r.Rider.Latitude,
                    Longitude: r.Rider.Longitude,
                    DistanceKm: Math.Round(r.Rider.StraightLineDistanceKm, 2),
                    Heading: r.Meta?.Heading,
                    Speed: r.Meta?.Speed,
                    ActiveOrders: r.Meta?.Load ?? 0,
                    Rating: 0,
                    VehicleType: profile?.VehicleType ?? "Motorcycle",
                    NumberPlate: profile?.NumberPlate ?? "—",
                    VehicleColor: profile?.VehicleColor ?? "—",
                    IsOnline: user?.IsOnline ?? false);
            })
            .Where(r => r is not null)
            .Cast<NearbyRiderDto>()
            .ToList();

        return Result.Success<IReadOnlyList<NearbyRiderDto>>(result);
    }
}
