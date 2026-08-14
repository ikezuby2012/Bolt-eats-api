using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Users.SubmitRiderProfile;

public sealed record SubmitRiderProfileCommand(
    Guid UserId,
    string NumberPlate,
    string VehicleType,
    string VehicleMake,
    string VehicleModel,
    string VehicleColor,
    string VehicleYear,
    string? DriverLicenseLink,
    string? NationalIdLink,
    string? VehiclePhotoLink,
    string? InsuranceCertLink
    ) : ICommand<RiderProfileDto>;
