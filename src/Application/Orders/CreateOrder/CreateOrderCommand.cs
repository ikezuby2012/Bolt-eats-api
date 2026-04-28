using Application.Abstractions.Messaging;
using Application.Orders.Dto;

namespace Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(string? CustomerNotes) : ICommand<OrderDto>;
