using SharedKernel;

namespace Domain.Notification;

public sealed class NotificationType : Enumeration<NotificationType>
{
    public static readonly NotificationType OrderPlaced = new(1, "Order Placed");
    public static readonly NotificationType OrderConfirmed = new(2, "Order Confirmed");
    public static readonly NotificationType OrderPreparing = new(3, "Order Preparing");
    public static readonly NotificationType OrderReadyForPickup = new(4, "Order Ready for Pickup");
    public static readonly NotificationType OrderOutForDelivery = new(5, "Order Out for Delivery");
    public static readonly NotificationType OrderDelivered = new(6, "Order Delivered");
    public static readonly NotificationType OrderCancelled = new(7, "Order Cancelled");

    // Payment Notifications
    public static readonly NotificationType PaymentSucceeded = new(8, "Payment Succeeded");
    public static readonly NotificationType PaymentFailed = new(9, "Payment Failed");

    // Promo & Review Notifications
    public static readonly NotificationType PromoCodeApplied = new(10, "Promo Code Applied");
    public static readonly NotificationType ReviewReceived = new(11, "Review Received");

    // General Notification
    public static readonly NotificationType General = new(12, "General");

    private NotificationType(int Id, string name) : base(Id, name) { }
}
