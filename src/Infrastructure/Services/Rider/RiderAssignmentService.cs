using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Notification;
using Application.Abstractions.Services.Rider;
using Application.Orders.OfferOrderToRider;
using Application.Tracking.Dto;
using Domain.Notification;
using Domain.Order;
using GoogleApi;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Common.Enums;
using GoogleApi.Entities.Maps.DistanceMatrix.Request;
using GoogleApi.Entities.Maps.DistanceMatrix.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Infrastructure.Services.Rider;

internal sealed class RiderAssignmentService(IApplicationDbContext context, IConfiguration config, IDateTimeProvider dateTimeProvider, ICommandHandler<OfferOrderToRiderCommand> offerHandler, IRiderLocationCache locationCache, INotificationService notificationService) : IRiderAssignmentService
{
    private static readonly double[] SearchRadiiKm = [3.0, 7.0, 15.0];

    private const int MaxCandidates = 8;
    private const int FallbackEtaMinutes = 20;

    // Scoring weights — must sum to 1.0
    private const double W_PickupEta = 0.4;
    private const double W_DeliveryEta = 0.4;
    private const double W_RiderLoad = 0.1;
    private const double W_RestaurantWait = 0.1;

    // Normalisation ceilings — anything above these is treated as maximum
    private const double EtaCeiling = 60.0;   // minutes
    private const double LoadCeiling = 5.0;    // concurrent orders
    private const double WaitCeiling = 10.0;
    private const int FallbackSeconds = 20 * 60;

    private sealed record RiderScore(Guid RiderId, double Score, int PickupEtaMinutes, int DeliveryEtaMinutes);

    public async Task<DistanceMatrixResult> GetDurationInTrafficAsync(GeoCoordinateDto origin, GeoCoordinateDto destination, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DistanceMatrixRequest
            {
                Key = config["Google:ApiKey"]!,
                Origins = [new LocationEx(new CoordinateEx(origin.Latitude, origin.Longitude))],
                Destinations = [new LocationEx(new CoordinateEx(destination.Latitude, destination.Longitude))],
                TravelMode = TravelMode.DRIVING,
                DepartureTime = dateTimeProvider.UtcNow,
                TrafficModel = TrafficModel.Best_Guess,
                Units = Units.Metric,
            };

            GoogleApi.Entities.Maps.DistanceMatrix.Response.DistanceMatrixResponse response = await GoogleMaps.DistanceMatrix.QueryAsync(
                request, cancellationToken);

            if (response.Status != Status.Ok)
            {
                return new DistanceMatrixResult(false, 0, 0, $"Google API error: {response.Status}");
            }

            GoogleApi.Entities.Maps.DistanceMatrix.Response.Element? element = response.Rows?.FirstOrDefault()?.Elements?.FirstOrDefault();

            if (element?.Status != Status.Ok)
            {
                return new DistanceMatrixResult(false, 0, 0, $"Route element error: {element?.Status}");
            }

            int durationSeconds = element.DurationInTraffic?.Value ?? element.Duration?.Value ?? 0;

            int distanceMetres = element.Distance?.Value ?? 0;

            return new DistanceMatrixResult(
                IsSuccess: true,
                DurationInTrafficSeconds: durationSeconds,
                DistanceMetres: distanceMetres);
        }
        catch (Exception ex)
        {
            return new DistanceMatrixResult(false, 0, 0, ex.Message);
        }
    }

    private async Task<List<DurationResult>> GetBatchDurationsAsync(List<GeoCoordinateDto> origins, GeoCoordinateDto destination, CancellationToken cancellationToken = default)
    {
        if (!origins.Any())
        { return []; }

        try
        {
            var allResults = new List<DurationResult>();

            // Google allows max 25 origins per request
            foreach (GeoCoordinateDto[] batch in origins.Chunk(25))
            {
                var request = new DistanceMatrixRequest
                {
                    Key = config["Google:ApiKey"]!,
                    TravelMode = TravelMode.DRIVING,
                    DepartureTime = DateTime.UtcNow,
                    TrafficModel = TrafficModel.Best_Guess,

                    Origins = batch
                        .Select(o => new LocationEx(
                            new CoordinateEx(o.Latitude, o.Longitude)))
                        .ToArray(),

                    Destinations = new[]
                    {
                        new LocationEx(new CoordinateEx(
                            destination.Latitude,
                            destination.Longitude))
                    }
                };

                GoogleApi.Entities.Maps.DistanceMatrix.Response.DistanceMatrixResponse response = await GoogleMaps.DistanceMatrix.QueryAsync(request, cancellationToken);

                if (response?.Status != Status.Ok ||
                    response.Rows is null)
                {
                    allResults.AddRange(
                        Enumerable.Repeat(Fallback(), batch.Length));
                    continue;
                }

                foreach (Row? row in response.Rows)
                {
                    Element? element = row.Elements?.FirstOrDefault();

                    if (element?.Status != Status.Ok)
                    {
                        allResults.Add(Fallback());
                        continue;
                    }

                    // DurationInTraffic is only present when DepartureTime is set
                    int seconds = element.DurationInTraffic?.Value
                               ?? element.Duration?.Value
                               ?? FallbackSeconds;

                    allResults.Add(new DurationResult(
                        IsSuccess: true,
                        DurationInTrafficSeconds: seconds,
                        IsTrafficBased: element.DurationInTraffic is not null));
                }
            }

            // Guarantee same count as input
            while (allResults.Count < origins.Count)
            {
                allResults.Add(Fallback());
            }


            return allResults;
        }
        catch
        {
            return Enumerable
                .Repeat(Fallback(), origins.Count)
                .ToList();
        }
    }

    public async Task TryAutoAssignAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        Order? order = await context.Order
        .Include(o => o.Address)
        .Include(o => o.Restaurant).ThenInclude(r => r.Addresses)
        .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            return;
        }

        if (order.RiderId.HasValue)
        { return; }

        Domain.Address.Address? restaurantAddress = order.Restaurant.Addresses.FirstOrDefault(a => a.IsDefault) ?? order.Restaurant.Addresses.FirstOrDefault();

        if (restaurantAddress is null)
        { return; }

        var restaurantCoord = new GeoCoordinateDto((double)(restaurantAddress.Latitude ?? 0m), (double)(restaurantAddress.Longitude ?? 0m));

        var customerCoord = new GeoCoordinateDto((double)(order.Address.Latitude ?? 0m), (double)(order.Address.Longitude ?? 0m));

        // Restaurant queue depth
        int queueDepth = await context.Order
            .CountAsync(o => o.RestaurantId == order.RestaurantId &&
                             (o.OrderStatusId == EOrderStatus.Accepted.Id ||
                              o.OrderStatusId == EOrderStatus.Preparing.Id),
                        cancellationToken);

        // Fetch nearby riders — excluding anyone who already declined
        List<NearbyRider> candidates = await FetchCandidatesExcludingAsync(
            restaurantCoord,
            [],
            cancellationToken);

        if (!candidates.Any())
        {
            // No riders available — notify restaurant and admin
            await notificationService.NotifyAsync(
                userId: order.Restaurant.OwnerId,
                NotificationTypeId: NotificationType.General.Id,
                NotificationChannelId: NotificationChannel.Both.Id,
                title: "No Riders Available",
                body: $"No riders available for order #{orderId.ToString()[..8]}. Please contact support.",
                payload: new { screen = "OwnerOrderDetail", orderId },
                cancellationToken: cancellationToken);
            return;
        }

        // Score and pick the best candidate
        List<RiderScore> scored = await ScoreAsync(
            candidates, restaurantCoord, customerCoord, queueDepth, cancellationToken);

        RiderScore? best = scored.MinBy(s => s.Score);
        if (best is null)
        { return; }

        await offerHandler.Handle(
            new OfferOrderToRiderCommand(orderId, best.RiderId),
            cancellationToken);
    }

    private async Task<List<NearbyRider>> FetchCandidatesExcludingAsync(GeoCoordinateDto restaurantCoord, List<Guid> excludeRiderIds, CancellationToken cancellationToken)
    {
        foreach (double radiusKm in SearchRadiiKm)
        {
            IReadOnlyList<NearbyRider> nearby = await locationCache.GetNearbyRidersAsync(
                restaurantCoord.Latitude,
                restaurantCoord.Longitude,
                radiusKm,
                MaxCandidates * 3,
                cancellationToken);

            var available = new List<NearbyRider>();

            foreach (NearbyRider rider in nearby)
            {
                // Skip declined riders
                if (excludeRiderIds.Contains(rider.RiderId))
                { continue; }


                RiderAvailability? status = await locationCache.GetStatusAsync(
                    rider.RiderId, cancellationToken);

                if (status == RiderAvailability.Available)
                { available.Add(rider); }

                if (available.Count == MaxCandidates)
                { break; }

            }

            if (available.Any())
            { return available; }
        }

        return [];
    }

    private async Task<List<RiderScore>> ScoreAsync(List<NearbyRider> candidates, GeoCoordinateDto restaurantCoord, GeoCoordinateDto customerCoord, int restaurantQueueDepth, CancellationToken cancellationToken)
    {
        // ── 1. Batch call — all rider positions → restaurant (one API call) ───
        // e.g. 8 riders → 1 restaurant = 1 Google API call, not 8
        List<DurationResult> pickupResults = await GetBatchDurationsAsync(
            candidates
                .Select(r => new GeoCoordinateDto(r.Latitude, r.Longitude))
                .ToList(),
            restaurantCoord,
            cancellationToken);

        // ── 2. Single call — restaurant → customer (same for all candidates) ──
        DistanceMatrixResult deliveryResult = await GetDurationInTrafficAsync(restaurantCoord, customerCoord, cancellationToken);

        int deliveryEtaMin = deliveryResult.IsSuccess
            ? (int)Math.Ceiling(deliveryResult.DurationInTrafficSeconds / 60.0)
            : FallbackEtaMinutes;

        // ── 3. Score each candidate ───────────────────────────────────────────
        var scores = new List<RiderScore>();

        for (int i = 0; i < candidates.Count; i++)
        {
            NearbyRider rider = candidates[i];

            // Pickup ETA for this specific rider
            int pickupEtaMin = pickupResults[i].IsSuccess
                ? (int)Math.Ceiling(pickupResults[i].DurationInTrafficSeconds / 60.0)
                : FallbackEtaMinutes;

            // Rider load from Redis — O(1), no DB query
            RiderMeta? meta = await locationCache.GetMetaAsync(
                rider.RiderId, cancellationToken);
            int riderLoad = meta?.Load ?? 0;

            // ── Normalise all components to 0–1 before weighting ─────────────
            // Without normalisation, a 45-min ETA would completely dominate
            // a load difference of 1 order — they are on different scales
            double normPickup = Math.Min(pickupEtaMin, EtaCeiling) / EtaCeiling;
            double normDelivery = Math.Min(deliveryEtaMin, EtaCeiling) / EtaCeiling;
            double normLoad = Math.Min(riderLoad, LoadCeiling) / LoadCeiling;
            double normWait = Math.Min(restaurantQueueDepth, WaitCeiling) / WaitCeiling;

            // ── Weighted score — lower = better ──────────────────────────────
            double score = normPickup * W_PickupEta        // 0.4
                      + normDelivery * W_DeliveryEta      // 0.4
                      + normLoad * W_RiderLoad        // 0.1
                      + normWait * W_RestaurantWait;  // 0.1

            scores.Add(new RiderScore(
                RiderId: rider.RiderId,
                Score: score,
                PickupEtaMinutes: pickupEtaMin,
                DeliveryEtaMinutes: deliveryEtaMin));
        }

        return scores;
    }

    private static DurationResult Fallback() =>
       new(IsSuccess: false,
           DurationInTrafficSeconds: 20 * 60,   // 20-minute default
           IsTrafficBased: false);

    public async Task MarkRiderBusyAsync(Guid riderId, CancellationToken cancellationToken = default)
    {
        await locationCache.SetStatusAsync(riderId, RiderAvailability.Busy, cancellationToken);

        // Increment active order count
        await locationCache.IncrementLoadAsync(riderId, cancellationToken);
    }
}
