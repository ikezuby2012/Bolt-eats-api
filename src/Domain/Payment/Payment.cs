using Domain.Users;
using SharedKernel;

namespace Domain.Payment;

public sealed class Payment : Auditable<Guid>
{
    public Guid OrderId { get; set; }
    public Order.Order Order { get; set; }
    public Guid CustomerId { get; set; }
    public User Customer { get; set; }
    public int GatewayId { get; set; }
    public PaymentGateway Gateway { get; set; }
    public int StatusId { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NGN";
    public long AmountInKobo { get; set; }

    /// <summary>Stripe PaymentIntent ID or Monnify transaction reference.</summary>
    public string GatewayReference { get; set; }
    public string? ClientSecret { get; set; }

    /// <summary>Stripe customer ID or Monnify customer code.</summary>
    public string? GatewayCustomerId { get; set; }
    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }
    public bool OrderCreationFailed { get; set; }
    public string? CustomerNotes { get; set; }

    public string? RefundReference { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundedAt { get; set; }
}
