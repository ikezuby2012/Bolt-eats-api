using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace Application.Orders.GetAllOrdersAdmin;

public class GetAllOrdersAdminQueryValidator : AbstractValidator<GetAllOrdersAdminQuery>
{
    public GetAllOrdersAdminQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        When(x => x.From.HasValue && x.To.HasValue, () =>
            RuleFor(x => x.To)
                .GreaterThan(x => x.From!.Value)
                .WithMessage("'To' must be after 'From'."));
    }
}
