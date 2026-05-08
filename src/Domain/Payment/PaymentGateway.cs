using SharedKernel;

namespace Domain.Payment;

public sealed class PaymentGateway : Enumeration<PaymentGateway>
{
    public static readonly PaymentGateway Stripe = new(1, "Stripe");
    public static readonly PaymentGateway Monnify = new(2, "Monnify");

    private PaymentGateway(int Id, string name) : base(Id, name) { }
}
