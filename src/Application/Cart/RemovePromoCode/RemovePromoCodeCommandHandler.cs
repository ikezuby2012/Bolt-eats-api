using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Domain.Cart;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Cart.RemovePromoCode;

internal sealed class RemovePromoCodeCommandHandler(IApplicationDbContext context, IUserContext userContext, ICartService cartService) : ICommandHandler<RemovePromoCodeCommand, CartSummaryDto>
{
    public async Task<Result<CartSummaryDto>> Handle(RemovePromoCodeCommand command, CancellationToken cancellationToken)
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

        cart.PromoCode = null;
        cart.PromoDiscount = null;
        cart.PromoDiscountType = null;

        await context.SaveChangesAsync(cancellationToken);

        CartSummaryDto summary = cartService.Calculate(cart);

        return summary;
    }
}
