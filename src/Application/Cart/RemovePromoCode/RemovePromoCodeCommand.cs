using Application.Abstractions.Messaging;
using Domain.Cart;

namespace Application.Cart.RemovePromoCode;

public sealed record RemovePromoCodeCommand(Guid CartId) : ICommand<CartSummaryDto>;
