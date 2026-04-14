using FluentValidation;

namespace Application.Restaurant.UpdateMenuItem;
public class UpdateMenuItemCommandValidation : AbstractValidator<UpdateMenuItemCommand>
{
    public UpdateMenuItemCommandValidation()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.MenuItemId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Price).GreaterThan(0);

        RuleFor(x => x.DiscountPrice)
            .GreaterThan(0)
            .LessThan(x => x.Price)
            .When(x => x.DiscountPrice.HasValue);

        RuleFor(x => x.Calories).GreaterThan(0).When(x => x.Calories.HasValue);
        RuleFor(x => x.PrepTimeMin).GreaterThan(0);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
