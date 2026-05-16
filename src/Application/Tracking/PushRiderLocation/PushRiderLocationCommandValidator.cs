using FluentValidation;

namespace Application.Tracking.PushRiderLocation;

public class PushRiderLocationCommandValidator
    : AbstractValidator<PushRiderLocationCommand>
{
    public PushRiderLocationCommandValidator()
    {
        RuleFor(x => x.RiderId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.Accuracy)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Accuracy.HasValue);

        RuleFor(x => x.Bearing)
            .InclusiveBetween(0, 360)
            .When(x => x.Bearing.HasValue);

        RuleFor(x => x.Speed)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Speed.HasValue);
    }
}
