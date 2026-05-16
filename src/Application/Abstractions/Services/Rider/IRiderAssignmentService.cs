using Application.Tracking.Dto;

namespace Application.Abstractions.Services.Rider;

public interface IRiderAssignmentService
{
    Task TryAutoAssignAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<DistanceMatrixResult> GetDurationInTrafficAsync(GeoCoordinateDto origin, GeoCoordinateDto destination, CancellationToken cancellationToken = default);
}
