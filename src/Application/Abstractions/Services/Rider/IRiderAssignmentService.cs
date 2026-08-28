using Application.Tracking.Dto;

namespace Application.Abstractions.Services.Rider;

public interface IRiderAssignmentService
{
    Task TryAutoAssignAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<DistanceMatrixResult> GetDurationInTrafficAsync(GeoCoordinateDto origin, GeoCoordinateDto destination, CancellationToken cancellationToken = default);
    Task MarkRiderBusyAsync(
        Guid riderId,
        CancellationToken cancellationToken = default);

    //Task<List<DurationResult>> GetBatchDurationsAsync(List<GeoCoordinate> origins,GeoCoordinate destination, CancellationToken cancellationToken = default);
}
