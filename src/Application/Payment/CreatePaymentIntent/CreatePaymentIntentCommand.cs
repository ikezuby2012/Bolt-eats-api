using Application.Abstractions.Messaging;
using Application.Payment.Dto;

namespace Application.Payment.CreatePaymentIntent;

public sealed record CreatePaymentIntentCommand(Guid OrderId, int GatewayId = 2) : ICommand<PaymentIntentDto>;
