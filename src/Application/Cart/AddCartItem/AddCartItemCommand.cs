using Application.Abstractions.Messaging;
using Application.Cart.Dto;

namespace Application.Cart.AddCartItem;

public sealed record class AddCartItemCommand(Guid MenuItemId, int Quantity, string? Notes, string? PromoCode) : ICommand<CartDto>;
