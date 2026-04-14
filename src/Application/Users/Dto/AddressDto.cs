using System.Text.Json.Serialization;

namespace Application.Users.Dto;

public sealed class AddressDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    [JsonPropertyName("latitudeRaw")]
    public string? LatitudeRaw { get; set; }

    [JsonPropertyName("longitudeRaw")]
    public string? LongitudeRaw { get; set; }

    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }


    public static explicit operator AddressDto(Domain.Address.Address address) => new AddressDto
    {
        Id = address.Id,
        UserId = address.UserId,
        Label = address.Label,
        Street = address.Street,
        City = address.City,
        State = address.State,
        Country = address.Country,
        PostalCode = address.PostalCode,
        Latitude = address.Latitude,
        Longitude = address.Longitude,
        LatitudeRaw = address.LatitudeRaw,
        LongitudeRaw = address.LongitudeRaw,
        IsDefault = address.IsDefault,
        CreatedAt = address.CreatedAt,
        UpdatedAt = address.UpdatedAt
    };
}
