using Domain.Payment;

namespace Application.Payment.Dto;

public record PaymentConfirmationDto(
    Guid PaymentId,
    Guid OrderId,
    string Status,
    decimal Amount,
    string Currency,
    DateTime PaidAt);
