using Application.Abstractions.Messaging;

namespace Application.Payment.ProcessRefund;

public sealed record ProcessRefundCommand(Guid OrderId, string Reason) : ICommand<string>;
