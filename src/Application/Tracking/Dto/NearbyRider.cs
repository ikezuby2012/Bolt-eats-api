namespace Application.Tracking.Dto;

public enum RiderAvailability { Available, Busy, Offline }

public record NearbyRider(
    Guid RiderId,
    double Latitude,
    double Longitude,
    double StraightLineDistanceKm);

public record RiderMeta(
    double? Heading,
    double? Speed,
    Guid? ActiveOrderId,
    DateTime UpdatedAt,
    int Load);
