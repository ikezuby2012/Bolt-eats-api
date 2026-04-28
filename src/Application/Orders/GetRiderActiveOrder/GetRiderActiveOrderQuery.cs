using Application.Abstractions.Messaging;
using Application.Orders.Dto;

namespace Application.Orders.GetRiderActiveOrder;

public sealed record GetRiderActiveOrderQuery() : IQuery<OrderDto>;
