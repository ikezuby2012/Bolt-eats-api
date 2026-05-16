using FluentValidation;

namespace Application.Promo.GetPromoCode;

public class GetPromoCodesQueryValidator
    : AbstractValidator<GetPromoCodeQuery>
{
    public GetPromoCodesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
