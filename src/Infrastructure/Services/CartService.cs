using Application.Abstractions.Services;
using Domain.Cart;

namespace Infrastructure.Services;

internal sealed class CartService : ICartService
{
    private const decimal TaxRate = 0.075m; /// 7.5%;
    public CartSummaryDto Calculate(Cart cart)
    {
        decimal subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

        decimal deliveryFee = cart.Restaurant.DeliveryFeeMin ?? 0m;
        bool freeDelivery = false;

        decimal promoDiscount = 0m;

        decimal taxableAmount = subtotal - promoDiscount;
        decimal tax = Math.Round(taxableAmount * TaxRate, 2);
        decimal total = taxableAmount + deliveryFee + tax;

        bool meetsMinimum = cart.Restaurant.MinOrderAmount is null ||
                    subtotal >= cart.Restaurant.MinOrderAmount;

        return new CartSummaryDto(
            Subtotal: subtotal,
            DeliveryFee: deliveryFee,
            PromoDiscount: promoDiscount,
            Tax: tax,
            Total: total,
            PromoCode: cart.PromoCode,
            FreeDelivery: freeDelivery, meetsMinimum);
    }
}
