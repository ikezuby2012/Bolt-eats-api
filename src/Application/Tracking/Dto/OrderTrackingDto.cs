namespace Application.Tracking.Dto;

public sealed record OrderTrackingDto(
    Guid OrderId,
    string Status,
    string StatusLabel,
    int ProgressStep,        // 0-4 maps to Flutter TrackingStatus steps
    string ArrivalTime,         // estimated arrival e.g. "10:15"
    string LatestArrivalTime,   // latest possible e.g. "10:40"
    string RestaurantName,
    string RestaurantAddress,
    double? RiderLatitude,
    double? RiderLongitude,
    double? RiderHeading,
    string? riderName,
    string? riderPlate,
    string? riderVehicle,
    string? riderAvatarImg,
    double? riderRating,
    string? RiderImg,
    double DeliveryLatitude,
    double DeliveryLongitude,
    string DeliveryAddress,
    string DeliveryType,        // "Leave at door" | "Hand it to me"
    string? Instructions,
    string ServiceType,         // "Standard" | "Express"
    decimal Total,
    IReadOnlyList<TrackingOrderItemDto> Items);

public sealed record TrackingOrderItemDto(
    string Name,
    int Qty,
    decimal UnitPrice);
