using Application.Abstractions.Messaging;
using Domain.Cart;

namespace Application.Cart.ApplyPromoCode;

public sealed record ApplyPromoCodeCommand(Guid CartId, string Code) : ICommand<CartSummaryDto>;
