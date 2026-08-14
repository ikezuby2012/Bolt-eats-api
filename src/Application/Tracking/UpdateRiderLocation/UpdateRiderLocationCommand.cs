using Application.Abstractions.Messaging;

namespace Application.Tracking.UpdateRiderLocation;

public sealed record UpdateRiderLocationCommand(
    Guid RiderId,
    double Latitude,
    double Longitude,
    double? Heading,
    double? Speed)
    : ICommand;

