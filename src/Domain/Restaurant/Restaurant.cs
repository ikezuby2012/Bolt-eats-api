using Domain.Address;
using Domain.Users;
using SharedKernel;

namespace Domain.Restaurant;

public sealed class Restaurant : Auditable<Guid>
{
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid? AddressId { get; set; }
    public Address.Address Address { get; set; }
    public double Rating { get; set; }
    public int TotalReviews { get; set; }
    public decimal? DeliveryFeeMin { get; set; }
    public decimal? DeliveryFeeMax { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? EstDeliveryMin { get; set; }
    public int? EstDeliveryMax { get; set; }
    public bool IsOpen { get; set; }
    public bool IsActive { get; set; }
    public bool UberOnePartner { get; set; }
}
