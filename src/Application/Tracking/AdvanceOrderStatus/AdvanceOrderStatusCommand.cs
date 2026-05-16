using Application.Abstractions.Messaging;

namespace Application.Tracking.AdvanceOrderStatus;

public sealed record AdvanceOrderStatusCommand(
     Guid RiderId,
    Guid OrderId,
    string NewStatus) : ICommand;
