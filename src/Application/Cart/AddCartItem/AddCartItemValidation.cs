using FluentValidation;

namespace Application.Cart.AddCartItem;

public class AddCartItemValidation : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemValidation()
    {
        RuleFor(x => x.MenuItemId).NotEmpty();

        RuleFor(x => x.Quantity)
             .GreaterThan(0)
             .LessThanOrEqualTo(20)
             .WithMessage("Quantity must be between 1 and 20. Use DELETE to remove an item.");
    }
}
