using System.Text.Json.Serialization;
using Application.Restaurant.Dto;
using Domain.Cart;
using SharedKernel;

namespace Application.Cart.Dto;

public class CartItemDto : IAuditable<Guid>
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public CartDto? Cart { get; set; }
    public Guid? MenuItemId { get; set; }
    public MenuItemDto? MenuItem { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    [JsonIgnore]
    public bool IsSoftDeleted { get; set; }

    public static explicit operator CartItemDto(CartItem cartItem) => new CartItemDto
    {
        Id = cartItem.Id,
        CartId = cartItem.CartId,
        Cart = cartItem.Cart != null ? (CartDto?)cartItem.Cart : null,
        MenuItemId = cartItem.MenuItemId,
        Quantity = cartItem.Quantity,
        UnitPrice = cartItem.UnitPrice,
        Notes = cartItem.Notes,
        MenuItem = cartItem.MenuItem != null ? (MenuItemDto?)cartItem.MenuItem : null,
        CreatedAt = cartItem.CreatedAt,
        CreatedBy = cartItem.CreatedBy,
        UpdatedAt = cartItem.UpdatedAt,
        UpdatedBy = cartItem.UpdatedBy,
    };
}
