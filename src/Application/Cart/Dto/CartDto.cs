using System.Text.Json.Serialization;
using Application.Restaurant.Dto;
using Application.Users.Dto;
using Domain.Address;
using Domain.Cart;
using Domain.Restaurant;
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
                .Select(a => (CartItemDto)a)
                .ToList()
    };
}
