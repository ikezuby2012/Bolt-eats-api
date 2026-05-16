using Application.Tracking.Dto;

namespace Application.Abstractions.Services.Rider;

public interface IRiderLocationCache
{
    Task UpdateLocationAsync(
        Guid riderId, double latitude, double longitude,
        double? heading, double? speed,
        CancellationToken cancellationToken = default);

    Task SetStatusAsync(
        Guid riderId, RiderAvailability status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NearbyRider>> GetNearbyRidersAsync(
        double latitude, double longitude, double radiusKm,
        int maxResults = 10,
        CancellationToken cancellationToken = default);

    Task<RiderAvailability?> GetStatusAsync(Guid riderId,
        CancellationToken cancellationToken = default);

    Task<RiderMeta?> GetMetaAsync(Guid riderId,
        CancellationToken cancellationToken = default);

    Task IncrementLoadAsync(Guid riderId, CancellationToken cancellationToken = default);
    Task DecrementLoadAsync(Guid riderId, CancellationToken cancellationToken = default);
}
