using System.Text.Json.Serialization;
using Domain.Payment;
using SharedKernel;

namespace Application.Payment.Dto;

public record PaymentIntentDto(
    Guid PaymentId,
    string GatewayReference,
    string ClientSecret,       // Flutter passes this to Stripe SDK
    decimal Amount,
    string Currency,
    PaymentGateway Gateway);

public class PaymentHistoryDto : IAuditable<Guid>
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string PaymentStatus { get; set; }
    public string PaymentGateway { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string? FailureCode { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    [JsonIgnore]
    public bool IsSoftDeleted { get; set; }

    public static explicit operator PaymentHistoryDto(Domain.Payment.Payment payment) => new PaymentHistoryDto
    {
        Id = payment.Id,
        OrderId = payment.OrderId,
        PaymentStatus = Domain.Payment.PaymentStatus.FromValue(payment.StatusId)!.Name,
        PaymentGateway = Domain.Payment.PaymentGateway.FromValue(payment.GatewayId)!.Name,
        Amount = payment.Amount,
        Currency = payment.Currency,
        FailureCode = payment.FailureCode,
        CreatedAt = payment.CreatedAt,
        UpdatedAt = payment.UpdatedAt,
        UpdatedBy = payment.UpdatedBy,
    };
}
