using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Payments;

namespace Application.Payment.DetachPaymentMethod;

public sealed record DetachPaymentMethodCommand(string PaymentMethodId, int GatewayId = 2) : ICommand;
