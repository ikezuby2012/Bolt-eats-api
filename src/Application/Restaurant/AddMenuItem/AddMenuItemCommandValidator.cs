using FluentValidation;

namespace Application.Restaurant.AddMenuItem;
public class AddMenuItemCommandValidator : AbstractValidator<AddMenuItemCommand>
{
    public AddMenuItemCommandValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();

        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero.");

        RuleFor(x => x.DiscountPrice)
            .GreaterThan(0)
            .LessThan(x => x.Price)
            .WithMessage("Discount price must be less than the regular price.")
            .When(x => x.DiscountPrice.HasValue);

        //RuleFor(x => x.Calories)
        //    .GreaterThan(0)
        //    .When(x => x.Calories.HasValue);

        RuleFor(x => x.PrepTimeMin)
            .GreaterThan(0)
            .WithMessage("Prep time must be at least 1 minute.");

        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
