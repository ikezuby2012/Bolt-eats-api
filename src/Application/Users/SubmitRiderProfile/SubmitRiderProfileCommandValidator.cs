using FluentValidation;

namespace Application.Users.SubmitRiderProfile;

public class SubmitRiderProfileCommandValidator : AbstractValidator<SubmitRiderProfileCommand>
{
    private static readonly string[] AllowedVehicleTypes = ["Motorcycle", "Bicycle", "Car", "Van"];

    public SubmitRiderProfileCommandValidator()
    {
        RuleFor(x => x.NumberPlate)
         .NotEmpty()
         .MaximumLength(20)
         .Matches(@"^[A-Z]{2,3}\s?\d{2,4}\s?[A-Z]{2,3}$")
         .WithMessage("Enter a valid Nigerian number plate e.g. PH 234 ABC.");

        RuleFor(x => x.VehicleType)
            .NotEmpty()
            .Must(t => AllowedVehicleTypes.Contains(t))
            .WithMessage("Vehicle type must be Motorcycle, Bicycle, Car, or Van.");

        RuleFor(x => x.VehicleMake).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VehicleModel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VehicleColor).NotEmpty().MaximumLength(30);

        RuleFor(x => x.VehicleYear)
            .NotEmpty()
            .Matches(@"^\d{4}$")
            .Must(y => int.TryParse(y, out int yr) &&
                       yr >= 2000 &&
                       yr <= DateTime.UtcNow.Year)
            .WithMessage($"Enter a valid year between 2000 and {DateTime.UtcNow.Year}.");
    }
}
