using Domain.Restaurant;
using Domain.Users;
using SharedKernel;
using NetTopologySuite.Geometries;

namespace Domain.Address;

public sealed class Address : Auditable<Guid>
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public Guid? RestaurantId { get; set; }
    public Restaurant.Restaurant? Restaurant { get; set; }
    public string Label { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
    public string LatitudeRaw { get; set; }
    public string LongitudeRaw { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? DeliveryInstructions { get; set; }
    public string? BuildingType { get; set; }
    public string? AddressLabel { get; set; }
    public Dictionary<string, string>? BuildingDetails { get; set; } = new();
    public bool IsDefault { get; set; }

    public Point? Location { get; set; }

    public static Point CreatePoint(double lat, double lng)
    {
        return new Point(lng, lat) { SRID = 4326 };
    }
}
