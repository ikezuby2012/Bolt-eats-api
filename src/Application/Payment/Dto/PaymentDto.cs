using Domain.Payment;

namespace Application.Payment.Dto;

public class PaymentDto
{
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
