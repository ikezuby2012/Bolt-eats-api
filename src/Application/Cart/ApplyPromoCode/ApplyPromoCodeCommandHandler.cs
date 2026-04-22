using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Domain.Cart;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using SharedKernel;

namespace Application.Cart.ApplyPromoCode;

internal sealed class ApplyPromoCodeCommandHandler(IApplicationDbContext context, IUserContext userContext, IPromoCodeService promoCodeService, ICartService cartService) : ICommandHandler<ApplyPromoCodeCommand, CartSummaryDto>
{
    public async Task<Result<CartSummaryDto>> Handle(ApplyPromoCodeCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Cart.Cart? cart = await context.Cart.Include(c => c.Restaurant).Include(c => c.Items).ThenInclude(i => i.MenuItem).FirstOrDefaultAsync(c => c.Id == command.CartId && c.UserId == userId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure<CartSummaryDto>(Domain.Common.CommonErrors.CustomErrorMessage("No Active cart"));
        }

        if (!cart.Items.Any())
        {
            return Result.Failure<CartSummaryDto>(Domain.Common.CommonErrors.CustomErrorMessage("Cannot Apply promo to an empty cart."));
        }

        Domain.PromoCode.PromoValidationResult promoResult = await promoCodeService.ValidatePromoCodeAsync(
            command.Code,
            cart.UserId,
            cart.RestaurantId,
            cart.Items.Sum(i => i.UnitPrice * i.Quantity),
            cancellationToken
            );

        if (!promoResult.IsValid)
        {
            return Result.Failure<CartSummaryDto>(Domain.Common.CommonErrors.CustomErrorMessage(promoResult.Reason ?? "something went wrong"));
        }

        cart.PromoCode = command.Code.ToUpperInvariant();
        cart.PromoDiscount = promoResult.DiscountValue;
        cart.PromoDiscountType = promoResult.DiscountType;

        await context.SaveChangesAsync(cancellationToken);

        CartSummaryDto summary = cartService.Calculate(cart);

        return summary;
    }
}
