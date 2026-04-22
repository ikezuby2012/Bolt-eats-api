using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.PromoCode;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Services;

internal sealed class PromoCodeService(IApplicationDbContext context, IDateTimeProvider dateTimeProvider) : IPromoCodeService
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "<Pending>")]
    public async Task<PromoValidationResult> ValidatePromoCodeAsync(string code, Guid userId, Guid restaurantId, decimal subTotal, CancellationToken cancellationToken = default)
    {
        PromoCode? promo = await context.PromoCode.AsNoTracking().FirstOrDefaultAsync(p => p.Code == code.ToUpperInvariant() && p.IsActive, cancellationToken);

        if (promo == null)
        {
            return Fail("Promo code is invalid or has expired.");
        }

        DateTime now = dateTimeProvider.UtcNow;
        if (now < promo.StartsAt)
        {
            return Fail("Promo code is not yet active.");
        }
        if (now > promo.ExpiresAt)
        {
            return Fail("Promo code has expired");
        }

        if (promo.RestaurantId.HasValue && promo.RestaurantId != restaurantId)
        {
            return Fail("Promo code is not valid for this restaurant.");
        }

        if (promo.UsageLimit.HasValue && promo.UsageCount >= promo.UsageLimit)
        {
            return Fail("Promo code has reached its usage limit.");
        }

        if (promo.UsageLimitPerUser.HasValue)
        {
            int userUsageCount = await context.PromoCodeUsages.CountAsync(
            u => u.PromoCodeId == promo.Id
              && u.UserId == userId
              && u.StatusId == Domain.PromoCode.PromoUsageStatus.Redeemed.Id,
            cancellationToken);

            if (userUsageCount >= promo.UsageLimitPerUser)
            {
                return Fail("You have already used this promo code the maximum number of times.");
            }
        }

        if (promo.MinOrderValue.HasValue && subTotal < promo.MinOrderValue)
        {
            return Fail($"A minimum order of {promo.MinOrderValue:C} is required to use this promo code.");
        }

        decimal discountValue = promo.DiscountType.ToUpperInvariant() == "PERCENTAGE" ? ComputePercentageDiscount(subTotal, promo.DiscountValue, promo.MaxDiscountCap) : promo.DiscountValue;

        return new PromoValidationResult(
            IsValid: true,
            Reason: null,
            DiscountValue: discountValue,
            DiscountType: promo.DiscountType);
    }

    private static PromoValidationResult Fail(string reason) =>
        new(IsValid: false, Reason: reason, DiscountValue: null, DiscountType: null);

    private static decimal ComputePercentageDiscount(
        decimal subtotal,
        decimal percentage,
        decimal? cap)
    {
        decimal raw = Math.Round(subtotal * (percentage / 100m), 2);
        return cap.HasValue ? Math.Min(raw, cap.Value) : raw;
    }
}
