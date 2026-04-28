using Application.Abstractions.Messaging;
using Application.Orders.Dto;

namespace Application.Orders.AssignRider;

public sealed record AssignRiderCommand(Guid OrderId, Guid RiderId) : ICommand<OrderDto>;
