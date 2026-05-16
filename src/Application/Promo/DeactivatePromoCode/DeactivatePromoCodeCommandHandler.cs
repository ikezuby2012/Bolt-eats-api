using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Promo.DeactivatePromoCode;

internal sealed class DeactivatePromoCodeCommandHandler(IApplicationDbContext db) : ICommandHandler<DeactivatePromoCodeCommand>
{
    public async Task<Result> Handle(DeactivatePromoCodeCommand command, CancellationToken cancellationToken)
    {
        Domain.PromoCode.PromoCode? promo = await db.PromoCode.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (promo is null)
        {
            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("Promo code not found."));
        }

        if (!promo.IsActive)
        {
            return Result.Success();   // idempotent
        }

        promo.IsActive = false;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
