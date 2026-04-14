using Domain.Users;
using SharedKernel;

namespace Domain.Review;

public sealed class Review : Auditable<Guid>
{
    public Guid OrderId { get; set; }
    public Order.Order Order { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid RestaurantId { get; set; }
    public Restaurant.Restaurant Restaurant { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
