using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace Application.Users.CreateNewAddress;

public sealed class CreateNewAddressCommandValidator
    : AbstractValidator<CreateNewAddressCommand>
{
    public CreateNewAddressCommandValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.State)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.LatitudeRaw)
            .NotEmpty()
            .Must(BeValidLatitude)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.LongitudeRaw)
            .NotEmpty()
            .Must(BeValidLongitude)
            .WithMessage("Longitude must be between -180 and 180.");
    }

    private bool BeValidLatitude(string value)
    {
        return double.TryParse(value, out double lat) && lat >= -90 && lat <= 90;
    }

    private bool BeValidLongitude(string value)
    {
        return double.TryParse(value, out double lng) && lng >= -180 && lng <= 180;
    }
}
