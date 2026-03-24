using Domain.Users;
using SharedKernel;

namespace Domain.Address;

public sealed class Address : Auditable<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; }
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
    public bool IsDefault { get; set; }
}
