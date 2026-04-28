using Application.Abstractions.Messaging;
using Application.Orders.Dto;

namespace Application.Orders.AdvanceOrderStatus;

public sealed record AdvanceOrderStatusCommand(Guid OrderId, string Status) : ICommand<OrderDto>;
