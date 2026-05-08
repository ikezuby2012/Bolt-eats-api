using Domain.Users;
using SharedKernel;

namespace Domain.Payment;

public sealed class PaymentStatus : Enumeration<PaymentStatus>
{
    public static readonly PaymentStatus Pending = new(1, "Pending");
    public static readonly PaymentStatus Processing = new(2, "Processing");
    public static readonly PaymentStatus Succeeded = new(3, "Succeeded");
    public static readonly PaymentStatus Failed = new(4, "Failed");
    public static readonly PaymentStatus Refunded = new(5, "Refunded");
    public static readonly PaymentStatus PartialRefund = new(6, "Partial Refund");
    public static readonly PaymentStatus Disputed = new(7, "Disputed");

    private PaymentStatus(int Id, string name) : base(Id, name) { }
}
