using System.Text.Json.Serialization;
using Application.Users.Dto;
using Domain.Users;

namespace Application.Restaurant.Dto;

public sealed class RestaurantDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("ownerId")]
    public Guid OwnerId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("logoUrl")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("bannerUrl")]
    public string? BannerUrl { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("address")]
    public AddressDto? Address { get; set; }

    [JsonPropertyName("addressId")]
    public Guid? AddressId { get; set; }

    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [JsonPropertyName("totalReviews")]
    public int TotalReviews { get; set; }

    [JsonPropertyName("deliveryFeeMin")]
    public decimal? DeliveryFeeMin { get; set; }

    [JsonPropertyName("deliveryFeeMax")]
    public decimal? DeliveryFeeMax { get; set; }

    [JsonPropertyName("minOrderAmount")]
    public decimal? MinOrderAmount { get; set; }

    [JsonPropertyName("estDeliveryMin")]
    public int? EstDeliveryMin { get; set; }

    [JsonPropertyName("estDeliveryMax")]
    public int? EstDeliveryMax { get; set; }

    [JsonPropertyName("isOpen")]
    public bool IsOpen { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("isCompanyPartner")]
    public bool IsCompanyPartner { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("updatedBy")]
    public string? UpdatedBy { get; set; }
    public ICollection<AddressDto>? Addresses { get; set; }

    public static explicit operator RestaurantDto(Domain.Restaurant.Restaurant restaurant) => new RestaurantDto
    {
        Id = restaurant.Id,
        OwnerId = restaurant.OwnerId,
        Name = restaurant.Name,
        Description = restaurant.Description,
        LogoUrl = restaurant.LogoUrl,
        BannerUrl = restaurant.BannerUrl,
        PhoneNumber = restaurant.PhoneNumber,
        Email = restaurant.Email,
        Rating = restaurant.Rating,
        TotalReviews = restaurant.TotalReviews,
        DeliveryFeeMin = restaurant.DeliveryFeeMin,
        DeliveryFeeMax = restaurant.DeliveryFeeMax,
        MinOrderAmount = restaurant.MinOrderAmount,
        EstDeliveryMin = restaurant.EstDeliveryMin,
        EstDeliveryMax = restaurant.EstDeliveryMax,
        IsOpen = restaurant.IsOpen,
        IsActive = restaurant.IsActive,
        IsCompanyPartner = restaurant.CompanyPartner,
        CreatedAt = restaurant.CreatedAt,
        UpdatedAt = restaurant.UpdatedAt,
        CreatedBy = restaurant.CreatedBy,
        UpdatedBy = restaurant.UpdatedBy,
        Addresses = restaurant.Addresses?
                .Select(a => (AddressDto)a)
                .ToList()
    };
}
