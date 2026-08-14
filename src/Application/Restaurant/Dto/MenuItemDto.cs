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

public sealed record MenuCategoryDto(
    Guid Id,
    string Name,
    int? DisplayOrder,
    IReadOnlyList<MenuItemDto2> Items);

public record HomeMenuItemDto(
    Guid MenuItemId,
    string MenuItemName,
    string? MenuItemImage,
    decimal Price,
    decimal? DiscountPrice,
    int PrepTimeMin,
    Guid RestaurantId,
    string RestaurantName,
    string? RestaurantLogo,
    double RestaurantRating,
    string DeliveryFee,
    string DeliveryTime, string? Description = "");

public record AfricanCuisineItemDto(
    Guid MenuItemId,
    string Name,
    decimal Price,
    decimal? DiscountPrice,
    string? ImageLink,
    string CategoryName,
    Guid RestaurantId,
    string RestaurantName,
    string? RestaurantLogo);

public sealed record HomeSectionItemDto(
    Guid Id,
    string Name,
    decimal Price,
    decimal? DiscountPrice,
    string? ImageLink,
    string CategoryName,
    Guid RestaurantId,
    string RestaurantName,
    string? RestaurantLogo,
    double RestaurantRating,
    string DeliveryFee,
    string DeliveryTime,
    int PrepTimeMin);

public sealed record NearbyCategoryDto(
    Guid CategoryId,
    string CategoryName,
    int SortOrder,
    Guid RestaurantId,
    string RestaurantName,
    string? RestaurantLogo,
    double RestaurantRating,
    bool RestaurantIsOpen,
    int? MenuItemCount = 0);

public sealed record NearbyMenuItemDto(
    Guid MenuItemId,
    string MenuItemName,
    string MenuItemDescription,
    decimal Price,
    decimal? DiscountPrice,
    string? ImageLink,
    int PrepTimeMin,
    bool IsPopular,
    int? Calories,
    Guid CategoryId,
    string CategoryName,
    Guid RestaurantId,
    string RestaurantName,
    string? RestaurantLogo,
    double RestaurantRating,
    bool RestaurantIsOpen,
    string DeliveryFee,
    string DeliveryTime);

public sealed record MenuItemDetailDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    string? ImageLink,
    int PrepTimeMin,
    bool IsPopular,
    bool IsAvailable,
    int? Calories,
    int SortOrder,
    Guid CategoryId,
    string CategoryName,
    Guid RestaurantId,
    string RestaurantName,
    string? RestaurantLogo,
    string? RestaurantBanner,
    double RestaurantRating,
    int RestaurantTotalReviews,
    bool RestaurantIsOpen,
    string DeliveryFee,
    string DeliveryTime,
    decimal? MinOrderAmount);

public sealed record RelatedMenuItemDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    string? ImageLink,
    int PrepTimeMin,
    bool IsPopular,
    string CategoryName);

public sealed record MenuItemDto2(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    string? ImageLink,
    int PrepTimeMin,
    bool IsPopular,
    bool IsAvailable,
    int? Calories,
    int SortOrder);
