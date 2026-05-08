using Application.Abstractions.Messaging;
using Application.Payment.Dto;
using Domain.Payment;

namespace Application.Payment.AttachPayment;


public sealed record AttachPaymentMethodCommand(string PaymemtMethodToken, int GatewayId = 2) : ICommand<PaymentMethodDto>;
