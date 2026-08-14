using Application.Abstractions.Messaging;
using Application.Orders.Dto;

namespace Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    Guid? AddressId,
    string? ContactEmail,
    string? ContactName,
    string? ContactPhone,
    string? CustomerNotes)
: ICommand<OrderDto>;
