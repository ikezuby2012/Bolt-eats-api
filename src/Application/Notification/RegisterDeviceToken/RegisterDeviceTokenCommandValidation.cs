using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FluentValidation;

namespace Application.Notification.RegisterDeviceToken;

public class RegisterDeviceTokenCommandValidation : AbstractValidator<RegisterDeviceTokenCommand>
{
    private static readonly string[] AllowedPlatforms =
        ["android", "ios", "web"];

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "<Pending>")]
    public RegisterDeviceTokenCommandValidation()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Platform)
            .NotEmpty()
            .Must(p => AllowedPlatforms.Contains(p.ToLowerInvariant()))
            .WithMessage("Platform must be android, ios, or web.");
    }
}
