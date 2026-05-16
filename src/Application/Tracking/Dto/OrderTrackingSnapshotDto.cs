using Application.Users.Dto;

namespace Application.Tracking.Dto;

public record OrderTrackingSnapshotDto(
    Guid OrderId,
    string Status,
    RiderLocationDto? RiderLocation,
    AddressDto DeliveryAddress,
    int? EstimatedMinutesRemaining);

public record OrderStatusChangedPayload(
    Guid OrderId,
    string OldStatus,
    string NewStatus,
    DateTime ChangedAt);
