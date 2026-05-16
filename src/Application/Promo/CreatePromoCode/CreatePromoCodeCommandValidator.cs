using FluentValidation;

namespace Application.Promo.CreatePromoCode;

public class CreatePromoCodeCommandValidator : AbstractValidator<CreatePromoCodeCommand>
{
    public CreatePromoCodeCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[A-Z0-9_\-]+$")
            .WithMessage("Code must contain only uppercase letters, numbers, hyphens, or underscores.");

        RuleFor(x => x.Description)
            .MaximumLength(300)
            .When(x => x.Description is not null);

        RuleFor(x => x.DiscountType).IsInEnum();

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0);

        // Percentage ceiling
        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType.Equals("PERCENTAGE", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Percentage discount cannot exceed 100.");

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinOrderAmount.HasValue);

        RuleFor(x => x.MaxDiscountCap)
            .GreaterThan(0)
            .When(x => x.MaxDiscountCap.HasValue);

        // MaxDiscountCap only makes sense for percentage promos
        RuleFor(x => x.MaxDiscountCap)
            .Null()
           .When(x => x.DiscountType.Equals("FLAT", StringComparison.OrdinalIgnoreCase))
            .WithMessage("MaxDiscountCap is only applicable to percentage-based promos.");

        RuleFor(x => x.UsageLimitTotal)
            .GreaterThan(0)
            .When(x => x.UsageLimitTotal.HasValue);

        RuleFor(x => x.UsageLimitPerUser)
            .GreaterThan(0)
            .When(x => x.UsageLimitPerUser.HasValue);

        // Per-user cap cannot exceed total cap
        RuleFor(x => x.UsageLimitPerUser)
            .LessThanOrEqualTo(x => x.UsageLimitTotal!.Value)
            .When(x => x.UsageLimitPerUser.HasValue && x.UsageLimitTotal.HasValue)
            .WithMessage("Per-user usage limit cannot exceed the total usage limit.");

        RuleFor(x => x.StartsAt)
            .LessThan(x => x.ExpiresAt)
            .WithMessage("StartsAt must be before ExpiresAt.");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("ExpiresAt must be in the future.");
    }
}
