using FluentValidation;

namespace Application.Restaurant.AddMenuCategory;

public class AddMenuCommandCategoryValidator : AbstractValidator<AddMenuCategoryCommand>
{
    public AddMenuCommandCategoryValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
