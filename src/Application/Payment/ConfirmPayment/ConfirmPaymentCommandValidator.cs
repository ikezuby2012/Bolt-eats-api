using FluentValidation;

namespace Application.Payment.ConfirmPayment;

public class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    public ConfirmPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();

        RuleFor(x => x.CustomerNotes)
            .MaximumLength(1000)
            .When(x => x.CustomerNotes is not null);
    }
}
