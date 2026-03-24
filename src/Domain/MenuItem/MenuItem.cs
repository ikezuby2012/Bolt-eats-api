using SharedKernel;

namespace Domain.MenuItem;

public sealed class MenuItem : Auditable<Guid>
{
    public Guid RestaurantId { get; set; }
    public Restaurant.Restaurant Restaurant { get; set; }
    public Guid CategoryId { get; set; }
    public Category.Category Category { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int? Calories { get; set; }
    public int PrepTimeMin { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsPopular { get; set; }
    public int SortOrder { get; set; }
}
