using System.Text.Json.Serialization;

namespace Application.Restaurant.Dto;
public sealed class CategoryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayOrder")]
    public int? DisplayOrder { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    public static explicit operator CategoryDto(Domain.Category.Category category) => new CategoryDto
    {
        Id = category.Id,
        Name = category.Name,
        DisplayOrder = category.DisplayOrder,
        IsActive = category.IsActive,
    };
}
