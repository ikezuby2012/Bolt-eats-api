using FluentValidation;

namespace Application.Orders.GetOrderHistory;

public class GetOrderHistoryQueryValidation : AbstractValidator<GetOrderHistoryQuery>
{
    public GetOrderHistoryQueryValidation()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("DateFrom cannot be in the future.")
            .When(x => x.DateFrom.HasValue);

        RuleFor(x => x.DateTo)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMonths(2)).WithMessage("DateTo cannot be more than 2 months.")
            .When(x => x.DateTo.HasValue);

        RuleFor(x => x)
            .Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateFrom <= x.DateTo)
            .WithMessage("DateFrom must be less than or equal to DateTo.");
    }
}
