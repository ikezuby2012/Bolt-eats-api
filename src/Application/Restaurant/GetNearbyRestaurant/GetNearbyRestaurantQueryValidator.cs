using FluentValidation;

namespace Application.Restaurant.GetNearbyRestaurant;

public class GetNearbyRestaurantQueryValidator : AbstractValidator<GetNearbyRestaurantQuery>
{
    public GetNearbyRestaurantQueryValidator()
    {
        RuleFor(x => x.lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.lng).InclusiveBetween(-180, 180);
        RuleFor(x => x.RadiusKm).GreaterThan(0).LessThanOrEqualTo(50);
        RuleFor(x => x.pageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 5000);
    }
}
