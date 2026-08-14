using Application.Abstractions.Messaging;
using Application.Payment.Dto;

namespace Application.Payment.GetPaymentStatus;

public sealed record GetPaymentStatusQuery(
    Guid PaymentId,
    Guid UserId)
    : IQuery<PaymentDto>;
