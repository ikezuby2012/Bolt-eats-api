using Domain.Users;
using SharedKernel;

namespace Domain.Rider;

public sealed class RiderLocation : Auditable<Guid>
{
    public Guid RiderId { get; set; }
    public User Rider { get; set; }
    public Guid OrderId { get; set; }
    public Order.Order Order { get; set; }
    public string LatitudeRaw { get; set; }
    public string LongitudeRaw { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public double Bearing { get; set; }
    public double Speed { get; set; }
    public double? Accuracy { get; set; }
    public DateTime? RecordedAt { get; set; }
}
