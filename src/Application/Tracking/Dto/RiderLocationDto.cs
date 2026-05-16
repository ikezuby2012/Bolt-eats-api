using System.Text.Json.Serialization;
using SharedKernel;

namespace Application.Tracking.Dto;

public sealed class RiderLocationDto : IAuditable<Guid>
{
    public Guid Id { get; set; }
    public Guid RiderId { get; set; }
    public Guid OrderId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitiude { get; set; }
    public double? Accuracy { get; set; }
    public double? Bearing { get; set; }
    public double? Speed { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    [JsonIgnore]
    public bool IsSoftDeleted { get; set; }


    public static explicit operator RiderLocationDto(Domain.Rider.RiderLocation riderLocation) => new RiderLocationDto
    {
        Id = riderLocation.Id,
        RiderId = riderLocation.RiderId,
        OrderId = riderLocation.OrderId,
        Latitude = riderLocation.Latitude,
        Longitiude = riderLocation.Longitude,
        Accuracy = riderLocation.Accuracy,
        Bearing = riderLocation.Bearing,
        Speed = riderLocation.Speed,
        CreatedAt = riderLocation.CreatedAt,
        CreatedBy = riderLocation.CreatedBy,
        UpdatedAt = riderLocation.UpdatedAt,
        UpdatedBy = riderLocation.UpdatedBy

    };
}
