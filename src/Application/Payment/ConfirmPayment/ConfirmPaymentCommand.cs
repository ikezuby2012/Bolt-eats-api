using Application.Abstractions.Messaging;
using Application.Payment.Dto;

namespace Application.Payment.ConfirmPayment;

public sealed record ConfirmPaymentCommand(Guid PaymentId, string? CustomerNotes) : ICommand<PaymentConfirmationDto>;
