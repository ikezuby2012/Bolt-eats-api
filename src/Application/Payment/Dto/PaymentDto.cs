using Domain.Payment;
using SharedKernel;

namespace Application.Payment.Dto;

public class PaymentDto : Auditable<Guid>
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public int GatewayId { get; set; }
    public int StatusId { get; set; }
    public string Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public long AmountInKobo { get; set; }
    public string GatewayReference { get; set; }
    public string? ClientSecret { get; set; }
    public string? GatewayCustomerId { get; set; }
    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }
    public bool OrderCreationFailed { get; set; }
    public string? CustomerNotes { get; set; }
    public string? RefundReference { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundedAt { get; set; }

    public static explicit operator PaymentDto(Domain.Payment.Payment payment)
    => new PaymentDto
    {
        Id = payment.Id,
        OrderId = payment.OrderId,
        CustomerId = payment.CustomerId,
        GatewayId = payment.GatewayId,
        StatusId = payment.StatusId,
        Status = PaymentStatus.FromValue(payment.StatusId)!.Name,
        Amount = payment.Amount,
        Currency = payment.Currency,
        AmountInKobo = payment.AmountInKobo,
        GatewayReference = payment.GatewayReference,
        ClientSecret = payment.ClientSecret,
        GatewayCustomerId = payment.GatewayCustomerId,
        FailureCode = payment.FailureCode,
        FailureMessage = payment.FailureMessage,
        OrderCreationFailed = payment.OrderCreationFailed,
        CustomerNotes = payment.CustomerNotes,
        RefundReference = payment.RefundReference,
        RefundAmount = payment.RefundAmount,
        RefundedAt = payment.RefundedAt,
        CreatedAt = payment.CreatedAt,
        CreatedBy = payment.CreatedBy,
        UpdatedAt = payment.UpdatedAt,
        UpdatedBy = payment.UpdatedBy
    };

}

public record CreateIntentRequest(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string GatewayCustomerId,
    string? Description = null,
    IDictionary<string, string>? Metadata = null);

public record CreateIntentResult(
    bool IsSuccess,
    string? GatewayReference,
    string? ClientSecret,       // Stripe only — sent to Flutter
    string? FailureCode = null,
    string? FailureMessage = null);

public record ConfirmPaymentResult(
    bool IsSuccess,
    PaymentStatus Status,
    string? FailureCode = null,
    string? FailureMessage = null);

public record RefundRequest(
    string GatewayReference,
    decimal Amount,
    string Reason);

public record RefundResult(
    bool IsSuccess,
    string? RefundReference = null,
    string? FailureMessage = null);

public record SavedPaymentMethod(
    string Id,
    string Brand,
    string Last4,
    int ExpMonth,
    int ExpYear,
    bool IsDefault);

public record AttachMethodResult(
    bool IsSuccess,
    string? FailureMessage = null);

public record PaymentMethodDto(
    string Id,
    string Brand,
    string Last4,
    int ExpMonth,
    int ExpYear,
    bool IsDefault);
