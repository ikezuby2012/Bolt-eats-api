using System.Text.Json.Serialization;

namespace Application.Restaurant.Dto;

public sealed class MenuItemDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("restaurantId")]
    public Guid RestaurantId { get; set; }

    [JsonPropertyName("categoryId")]
    public Guid CategoryId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("discountPrice")]
    public decimal? DiscountPrice { get; set; }

    [JsonPropertyName("effectivePrice")]
    public decimal EffectivePrice => DiscountPrice ?? Price;

    [JsonPropertyName("hasDiscount")]
    public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("calories")]
    public int? Calories { get; set; }

    [JsonPropertyName("prepTimeMin")]
    public int PrepTimeMin { get; set; }

    [JsonPropertyName("prepTimeDisplay")]
    public string PrepTimeDisplay =>
        PrepTimeMin < 60 ? $"{PrepTimeMin} min" : $"{PrepTimeMin / 60}h {PrepTimeMin % 60}m";


    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonPropertyName("isPopular")]
    public bool IsPopular { get; set; }

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("category")]
    public CategoryDto? Category { get; set; }

    [JsonPropertyName("restaurant")]
    public RestaurantDto? Restaurant { get; set; }


    public static explicit operator MenuItemDto(Domain.MenuItem.MenuItem item) => new MenuItemDto
    {
        Id = item.Id,
        RestaurantId = item.RestaurantId,
        CategoryId = item.CategoryId,
        Name = item.Name,
        Description = item.Description,
        Price = item.Price,
        DiscountPrice = item.DiscountPrice,
        ImageUrl = item.ImageUrl,
        Calories = item.Calories,
        PrepTimeMin = item.PrepTimeMin,
        IsAvailable = item.IsAvailable,
        IsPopular = item.IsPopular,
        SortOrder = item.SortOrder,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt,
        Category = item.Category != null ? (CategoryDto)item.Category : null,
        Restaurant = item.Restaurant != null ? (RestaurantDto)item.Restaurant : null
    };
}
