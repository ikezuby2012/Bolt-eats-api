using Domain.Restaurant;
using Domain.Users;
using SharedKernel;

namespace Domain.Cart;

public sealed class Cart : Auditable<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid RestaurantId { get; set; }
    public Restaurant.Restaurant Restaurant { get; set; }
    public string? PromoCode { get; set; }
    public decimal? PromoDiscount { get; set; }
    public string? PromoDiscountType { get; set; }
    public ICollection<CartItem> Items { get; set; } = [];
}
