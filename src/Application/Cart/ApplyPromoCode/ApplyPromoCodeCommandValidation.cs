using FluentValidation;

namespace Application.Cart.ApplyPromoCode;

public class ApplyPromoCodeCommandValidation : AbstractValidator<ApplyPromoCodeCommand>
{
    public ApplyPromoCodeCommandValidation()
    {
        RuleFor(x => x.CartId).NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[A-Z0-9_\-]+$")
            .WithMessage("Promo code must contain only uppercase letters, numbers, hyphens, or underscores.");
    }
}
