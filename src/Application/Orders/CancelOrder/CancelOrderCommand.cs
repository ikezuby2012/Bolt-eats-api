using Application.Abstractions.Messaging;

namespace Application.Orders.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, string? Reason) : ICommand<Guid>;
