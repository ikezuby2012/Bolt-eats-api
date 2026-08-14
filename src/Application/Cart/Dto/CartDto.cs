using System.Text.Json.Serialization;
using Application.Restaurant.Dto;
using Application.Users.Dto;
using SharedKernel;

namespace Application.Cart.Dto;

public class CartDto : IAuditable<Guid>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public CreatedUserDto? User { get; set; }
    public Guid RestaurantId { get; set; }
    public RestaurantDto? Restaurant { get; set; }
    public string? PromoCode { get; set; }
    public IEnumerable<CartItemDto>? items { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    [JsonIgnore]
    public bool IsSoftDeleted { get; set; }

    public static explicit operator CartDto(Domain.Cart.Cart cart) => new CartDto
    {
        Id = cart.Id,
        UserId = cart.UserId,
        User = cart.User != null ? (CreatedUserDto)cart.User : null,
        RestaurantId = cart.RestaurantId,
        Restaurant = cart.Restaurant != null ? (RestaurantDto)cart.Restaurant : null,
        PromoCode = cart.PromoCode,
        CreatedAt = cart.CreatedAt,
        CreatedBy = cart.CreatedBy,
        UpdatedAt = cart.UpdatedAt,
        UpdatedBy = cart.UpdatedBy,
        items = cart.Items?
        .Select(a => new CartItemDto
        {
            Id = a.Id,
            CartId = a.CartId,
            MenuItemId = a.MenuItemId,
            Quantity = a.Quantity,
            UnitPrice = a.UnitPrice,
            Notes = a.Notes,
            CreatedAt = a.CreatedAt,
            CreatedBy = a.CreatedBy,
            UpdatedAt = a.UpdatedAt,
            UpdatedBy = a.UpdatedBy,
            MenuItem = a.MenuItem == null ? null : new MenuItemDto
            {
                Id = a.MenuItem.Id,
                RestaurantId = a.MenuItem.RestaurantId,
                CategoryId = a.MenuItem.CategoryId,
                Name = a.MenuItem.Name,
                Description = a.MenuItem.Description,
                Price = a.MenuItem.Price,
                DiscountPrice = a.MenuItem.DiscountPrice,
                ImageUrl = a.MenuItem.ImageUrl,
                Calories = a.MenuItem.Calories,
                PrepTimeMin = a.MenuItem.PrepTimeMin,
                IsAvailable = a.MenuItem.IsAvailable,
                IsPopular = a.MenuItem.IsPopular,
                SortOrder = a.MenuItem.SortOrder,
                CreatedAt = a.MenuItem.CreatedAt,
                UpdatedAt = a.MenuItem.UpdatedAt
            }
        })
        .ToList()
    };
}
