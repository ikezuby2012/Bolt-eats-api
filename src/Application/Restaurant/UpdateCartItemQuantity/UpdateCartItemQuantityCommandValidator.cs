using FluentValidation;

namespace Application.Restaurant.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.CartItemId).NotEmpty();
        RuleFor(x => x.Action).IsInEnum();
    }
}
