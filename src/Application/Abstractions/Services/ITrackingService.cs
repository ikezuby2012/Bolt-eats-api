using Application.Tracking.Dto;
using Domain.Rider;

namespace Application.Abstractions.Services;

public interface ITrackingService
{
    Task BroadcastLocationAsync(
        Guid orderId,
        RiderLocationUpdatedPayload payload,
        CancellationToken cancellationToken = default);

    Task BroadcastStatusChangeAsync(
        Guid orderId,
        OrderStatusChangedPayload payload,
        CancellationToken cancellationToken = default);
}
