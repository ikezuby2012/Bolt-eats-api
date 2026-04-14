using FluentValidation;

namespace Application.Restaurant.UpdateRestaurantInfo;

public class UpdateRestaurantCommandValidator : AbstractValidator<UpdateRestaurantCommand>
{
    public UpdateRestaurantCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[1-9]\d{6,14}$");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => x.Email is not null);

        RuleFor(x => x.DeliveryFeeMin)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DeliveryFeeMin.HasValue);

        RuleFor(x => x.DeliveryFeeMax)
            .GreaterThanOrEqualTo(x => x.DeliveryFeeMin ?? 0)
            .When(x => x.DeliveryFeeMax.HasValue);

        RuleFor(x => x.EstDeliveryMin).GreaterThan(0).When(x => x.EstDeliveryMin.HasValue);
        RuleFor(x => x.EstDeliveryMax)
            .GreaterThan(x => x.EstDeliveryMin ?? 0)
            .When(x => x.EstDeliveryMax.HasValue);
    }
}
