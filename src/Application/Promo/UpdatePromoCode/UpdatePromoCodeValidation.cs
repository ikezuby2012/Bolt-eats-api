using FluentValidation;

namespace Application.Promo.UpdatePromoCode;

public class UpdatePromoCodeValidation : AbstractValidator<UpdatePromoCodeCommand>
{
    public UpdatePromoCodeValidation()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Description)
            .MaximumLength(300)
            .When(x => x.Description is not null);

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinOrderAmount.HasValue);

        RuleFor(x => x.MaxDiscountCap)
            .GreaterThan(0)
            .When(x => x.MaxDiscountCap.HasValue);

        RuleFor(x => x.UsageLimitTotal)
            .GreaterThan(0)
            .When(x => x.UsageLimitTotal.HasValue);

        RuleFor(x => x.UsageLimitPerUser)
            .GreaterThan(0)
            .When(x => x.UsageLimitPerUser.HasValue);

        When(x => x.StartsAt.HasValue && x.ExpiresAt.HasValue, () =>
            RuleFor(x => x.ExpiresAt)
                .GreaterThan(x => x.StartsAt!.Value)
                .WithMessage("ExpiresAt must be after StartsAt."));

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("ExpiresAt must be in the future.");

    }
}
