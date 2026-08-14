using Application.Abstractions.Messaging;
using Application.Cart.Dto;

namespace Application.Restaurant.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityCommand(Guid CartItemId,
    QuantityAction Action)
    : ICommand<CartDto>;

public enum QuantityAction { Increase, Decrease }
