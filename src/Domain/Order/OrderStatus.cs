using SharedKernel;

namespace Domain.Order;
public sealed class EOrderStatus : Enumeration<EOrderStatus>
{
    public static readonly EOrderStatus Pending = new(1, "Pending", "Order placed, awaiting restaurant acceptance");
    public static readonly EOrderStatus Accepted = new(2, "Accepted", "Restaurant confirmed the order");
    public static readonly EOrderStatus Preparing = new(3, "Preparing", "Food is being prepared");
    public static readonly EOrderStatus ReadyForPickup = new(4, "Ready_For_Pickup", "Ready for rider collection");
    public static readonly EOrderStatus InTransit = new(5, "In_Transit", "Rider en route to customer");
    public static readonly EOrderStatus Delivered = new(6, "Delivered", "Order completed Successfully");
    public static readonly EOrderStatus Cancelled = new(7, "Cancelled", "Cancelled by customer or restaurant");
    public static readonly EOrderStatus Refunded = new(8, "Refunded", "Payment reversed");

    public string Description { get; }

    private EOrderStatus(int Id, string name, string description) : base(Id, name)
    {
        Description = description;
    }
}
