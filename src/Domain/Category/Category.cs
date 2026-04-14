using Domain.Restaurant;
using SharedKernel;

namespace Domain.Category;

public sealed class Category : Auditable<Guid>
{
    public string Name { get; set; }
    public Guid RestaurantId { get; set; }
    public Restaurant.Restaurant Restaurant { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
