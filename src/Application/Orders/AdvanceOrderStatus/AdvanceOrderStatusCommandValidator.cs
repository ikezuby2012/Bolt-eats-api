using Domain.Order;
using FluentValidation;

namespace Application.Orders.AdvanceOrderStatus;

public class AdvanceOrderStatusCommandValidator : AbstractValidator<AdvanceOrderStatusCommand>
{
    public AdvanceOrderStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .Must(EOrderStatus.IsValidName)
            .WithMessage($"Name must be one of: {string.Join(", ", EOrderStatus.GetNames())}");

        RuleFor(x => x.Status)
            .NotEqual(EOrderStatus.Cancelled.Name)
            .WithMessage("Use the dedicated cancel endpoint to cancel an order.")
            .NotEqual(EOrderStatus.Pending.Name)
            .WithMessage("Cannot transition back to Pending.");
    }
}
