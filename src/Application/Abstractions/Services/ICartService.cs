using Domain.Cart;

namespace Application.Abstractions.Services;

public interface ICartService
{
    CartSummaryDto Calculate(Domain.Cart.Cart cart);
}
