using Application.Abstractions.Messaging;
using Domain.Cart;

namespace Application.Cart.GetCartSummary;

public sealed record GetCartSummaryQuery() : IQuery<CartSummaryDto>;
