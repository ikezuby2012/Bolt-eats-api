using Application.Abstractions.Messaging;
using Application.Cart.Dto;

namespace Application.Cart.UpdateCartItem;

public sealed record UpdateCartItemCommand(Guid ItemId, int Quantity) : ICommand<CartDto>;
