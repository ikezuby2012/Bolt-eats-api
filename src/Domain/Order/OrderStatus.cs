using SharedKernel;

namespace Domain.Order;
public sealed class EOrderStatus : Enumeration<EOrderStatus>
{
    public static readonly EOrderStatus AwaitingPayment =
         new(1, "Awaiting_Payment", "Order created, waiting for customer payment");

    public static readonly EOrderStatus Pending =
        new(2, "Pending", "Payment received, awaiting restaurant acceptance");

    public static readonly EOrderStatus Accepted =
        new(3, "Accepted", "Restaurant confirmed the order");

    public static readonly EOrderStatus Preparing =
        new(4, "Preparing", "Food is being prepared");

    public static readonly EOrderStatus ReadyForPickup =
        new(5, "Ready_For_Pickup", "Ready for rider collection");

    public static readonly EOrderStatus InTransit =
        new(6, "In_Transit", "Rider en route to customer");

    public static readonly EOrderStatus Delivered =
        new(7, "Delivered", "Order completed successfully");

    public static readonly EOrderStatus Cancelled =
        new(8, "Cancelled", "Cancelled by customer or restaurant");

    public static readonly EOrderStatus Refunded =
        new(9, "Refunded", "Payment reversed");

    public static readonly EOrderStatus PaymentFailed =
       new(10, "Payment_Failed", "Payment failed");

    public string Description { get; }

    private EOrderStatus(int Id, string name, string description) : base(Id, name)
    {
        Description = description;
    }

    public static List<string> GetNames()
    {
        return GetValues().Select(x => x.Name).ToList();
    }

    public static bool IsValidName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        { return false; }

        return GetValues()
            .Any(x => x.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
    }

    public static EOrderStatus? FromNameOrDefault(string input)
    {
        return GetValues()
            .FirstOrDefault(x => x.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
    }
}
