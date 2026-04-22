using Application.Abstractions.Messaging;
using Application.Cart.Dto;

namespace Application.Cart.DeleteCartItem;

public sealed record DeleteCartItemCommand(Guid ItemId) : ICommand<CartDto>;
