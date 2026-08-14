using Domain.Users;
using SharedKernel;

namespace Domain.Order;

public sealed class Order : Auditable<Guid>
{
    public Guid CustomerId { get; set; }
    public User Customer { get; set; }
    public Guid RestaurantId { get; set; }
    public Restaurant.Restaurant Restaurant { get; set; }
    public Guid CartId { get; set; }
    public Cart.Cart Cart { get; set; }
    public Guid? RiderId { get; set; }
    public User Rider { get; set; }
    public Guid? OfferedToRiderId { get; set; }
    public User OfferedToRider { get; set; }
    public Guid AddressId { get; set; }
    public Address.Address Address { get; set; }
    public int OrderStatusId { get; set; }
    public EOrderStatus OrderStatus { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal? Discount { get; set; }
    public decimal Tax { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactName { get; set; }
    public decimal Total { get; set; }
    public string? PromoCode { get; set; }
    public string? PaymentRef { get; set; }
    public string? Notes { get; set; }
    public string? CancellationNotes { get; set; }
    public string OrderCode { get; set; }
    public DateTime? CheckoutAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? OfferedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public int? EstimatedDeliveryMinutes { get; set; }
    public int? EstimatedTravelMinutes { get; set; }
    //public DateTime PreparingAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];
}
