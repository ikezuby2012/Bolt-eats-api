using Application.Abstractions.Messaging;
using Application.Payment.Dto;
using Domain.Payment;

namespace Application.Payment.GetPaymentMethods;

public sealed record GetPaymentMethodQuery(Guid UserId, int GatewayId = 2) : IQuery<IReadOnlyList<PaymentMethodDto>>;
