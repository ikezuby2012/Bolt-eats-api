using Application.Abstractions.Messaging;
using Application.Orders.Dto;

namespace Application.Orders.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto>;
