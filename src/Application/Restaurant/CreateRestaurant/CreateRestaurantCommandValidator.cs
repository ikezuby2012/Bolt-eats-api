using FluentValidation;

namespace Application.Restaurant.CreateRestaurant;

public class CreateRestaurantCommandValidator : AbstractValidator<CreateRestaurantCommand>
{
    public CreateRestaurantCommandValidator()
    {
        RuleFor(x => x.Name)
           .NotEmpty()
           .MaximumLength(150);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[1-9]\d{6,14}$")
            .WithMessage("Phone number must be a valid international format.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => x.Email is not null);

        RuleFor(x => x.DeliveryFeeMin)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DeliveryFeeMin.HasValue);

        RuleFor(x => x.DeliveryFeeMax)
            .GreaterThanOrEqualTo(x => x.DeliveryFeeMin ?? 0)
            .When(x => x.DeliveryFeeMax.HasValue);

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinOrderAmount.HasValue);

        RuleFor(x => x.EstDeliveryMin)
            .GreaterThan(0)
            .When(x => x.EstDeliveryMin.HasValue);

        RuleFor(x => x.EstDeliveryMax)
            .GreaterThan(x => x.EstDeliveryMin ?? 0)
            .When(x => x.EstDeliveryMax.HasValue);

        RuleFor(x => x.Address).NotNull().SetValidator(new AddressRequestValidator());
    }
}

public class AddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public AddressRequestValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Lng).InclusiveBetween(-180, 180);
    }
}
