using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Promo.Dto;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using SharedKernel;

namespace Application.Promo.UpdatePromoCode;

internal sealed class UpdatePromoCodeCommandHandler(IApplicationDbContext db) : ICommandHandler<UpdatePromoCodeCommand, PromoCodeDto>
{
    public async Task<Result<PromoCodeDto>> Handle(UpdatePromoCodeCommand command, CancellationToken cancellationToken)
    {
        Domain.PromoCode.PromoCode? promo = await db.PromoCode
            .Include(p => p.Restaurant)
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (promo is null)
        {
            return Result.Failure<PromoCodeDto>(Domain.Common.CommonErrors.CustomErrorMessage("Promo code not found."));
        }

        if (command.Description is not null)
        {
            promo.Description = command.Description;
        }

        if (command.MinOrderAmount is not null)
        {
            promo.MinOrderValue = command.MinOrderAmount;
        }

        if (command.MaxDiscountCap is not null)
        {
            promo.MaxDiscountCap = command.MaxDiscountCap;
        }

        if (command.UsageLimitTotal is not null)
        {
            promo.UsageLimit = command.UsageLimitTotal;
        }

        if (command.UsageLimitPerUser is not null)
        {
            promo.UsageLimitPerUser = command.UsageLimitPerUser;
        }
        if (command.StartsAt is not null)
        {
            promo.StartsAt = command.StartsAt.Value;
        }
        if (command.ExpiresAt is not null)
        {
            promo.ExpiresAt = command.ExpiresAt.Value;
        }

        await db.SaveChangesAsync(cancellationToken);

        return (PromoCodeDto)promo;
    }
}
