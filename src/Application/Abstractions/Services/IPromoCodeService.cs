using Domain.PromoCode;

namespace Application.Abstractions.Services;

public interface IPromoCodeService
{
    Task<PromoValidationResult> ValidatePromoCodeAsync(string code, Guid userId, Guid restaurantId, decimal subTotal, CancellationToken cancellationToken = default);
}
