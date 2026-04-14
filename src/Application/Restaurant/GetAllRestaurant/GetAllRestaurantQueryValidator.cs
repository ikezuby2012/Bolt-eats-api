using FluentValidation;

namespace Application.Restaurant.GetAllRestaurant;
public class GetAllRestaurantQueryValidator : AbstractValidator<GetAllRestaurantQuery>
{
    public GetAllRestaurantQueryValidator()
    {
        RuleFor(x => x.PageSize)
         .GreaterThan(1).WithMessage("PageSize must be greater than 0.")
         .LessThanOrEqualTo(1000).WithMessage("PageSize cannot exceed 1000.");

        RuleFor(x => x.pageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be greater than or equal to 1.");

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
