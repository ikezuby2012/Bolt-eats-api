using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Promo.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Promo.ValidatePromoCode;

internal sealed class ValidatePromoCodeCommandHandler(IApplicationDbContext db, IPromoCodeService promoService, IUserContext userContext) : ICommandHandler<ValidatePromoCodeCommand, PromoValidationResultDto>
{
    public async Task<Result<PromoValidationResultDto>> Handle(ValidatePromoCodeCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.Cart.Cart? cart = await db.Cart
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(
                c => c.UserId == userId,
                cancellationToken);

        if (cart is null || cart.Items.Any())
        {
            return Result.Failure<PromoValidationResultDto>(Domain.Common.CommonErrors.CustomErrorMessage("No active cart found. Add items before applying a promo code."));
        }

        decimal subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        Domain.PromoCode.PromoValidationResult result = await promoService.ValidatePromoCodeAsync(
            command.code,
            userId,
            cart.RestaurantId,
            subtotal,
            cancellationToken);

        decimal? resolved = null;
        if (result.IsValid && result.DiscountValue.HasValue)
        {
            resolved = result.DiscountType?.ToUpperInvariant() == "PERCENTAGE"
                ? Math.Round(subtotal * (result.DiscountValue.Value / 100m), 2)
                : result.DiscountValue.Value;

            Domain.PromoCode.PromoCode? promo = await db.PromoCode
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => EF.Functions.Like(p.Code, command.code),
                    cancellationToken);

            if (promo?.MaxDiscountCap.HasValue == true)
            {
                resolved = Math.Min(resolved.Value, promo.MaxDiscountCap.Value);
            }

        }

        return new PromoValidationResultDto(
            IsValid: result.IsValid,
            Reason: result.Reason,
            Code: result.IsValid ? command.code.ToUpperInvariant() : null,
            DiscountType: result.DiscountType,
            DiscountValue: result.DiscountValue,
            ResolvedDiscount: resolved
        );
    }
}
