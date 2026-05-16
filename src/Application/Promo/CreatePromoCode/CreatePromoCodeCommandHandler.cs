using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Promo.Dto;
using Domain.PromoCode;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Promo.CreatePromoCode;

internal sealed class CreatePromoCodeCommandHandler(IApplicationDbContext db) : ICommandHandler<CreatePromoCodeCommand, PromoCodeDto>
{
    public async Task<Result<PromoCodeDto>> Handle(CreatePromoCodeCommand command, CancellationToken cancellationToken)
    {
        string normalised = command.Code.ToUpperInvariant();

        bool exists = await db.PromoCode.AnyAsync(p => p.Code == normalised, cancellationToken);

        if (exists)
        {
            return Result.Failure<PromoCodeDto>(Domain.Common.CommonErrors.CustomErrorMessage($"A promo code {normalised} already exists."));
        }

        if (command.RestaurantId.HasValue)
        {
            bool restaurantExists = await db.Restaurants
                 .AnyAsync(
                     r => r.Id == command.RestaurantId && r.IsActive,
                     cancellationToken);

            if (!restaurantExists)
            {
                return Result.Failure<PromoCodeDto>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant not found."));
            }
        }

        var promo = new PromoCode
        {
            Id = Guid.NewGuid(),
            Code = normalised,
            Description = command.Description ?? "",
            DiscountType = command.DiscountType,
            DiscountValue = command.DiscountValue,
            MinOrderValue = command.MinOrderAmount,
            MaxDiscountCap = command.MaxDiscountCap,
            RestaurantId = command.RestaurantId,
            UsageCount = 0,
            UsageLimit = command.UsageLimitTotal,
            UsageLimitPerUser = command.UsageLimitPerUser,
            StartsAt = command.StartsAt,
            ExpiresAt = command.ExpiresAt,
            IsActive = true
        };

        db.PromoCode.Add(promo);
        await db.SaveChangesAsync(cancellationToken);

        return (PromoCodeDto)promo;
    }
}
