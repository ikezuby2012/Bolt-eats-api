namespace Application.Tracking.Dto;

public sealed record GeoCoordinateDto(double Latitude, double Longitude);

public sealed record DistanceMatrixResult(
    bool IsSuccess,
    int DurationInTrafficSeconds,
    int DistanceMetres,
    string? ErrorMessage = null);

public sealed record DeliveryEstimate(
    int TotalMinutes,
    int PrepMinutes,
    int TravelMinutes,
    int BufferMinutes,
    int DistanceMetres,
    bool IsTrafficBased);

public record DurationResult(
    bool IsSuccess,
    int DurationInTrafficSeconds,
    bool IsTrafficBased = true);
