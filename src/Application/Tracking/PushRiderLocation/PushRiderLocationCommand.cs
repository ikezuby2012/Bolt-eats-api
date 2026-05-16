using Application.Abstractions.Messaging;

namespace Application.Tracking.PushRiderLocation;

public sealed record PushRiderLocationCommand(
    Guid RiderId,
    Guid OrderId,
    double Latitude,
    double Longitude,
    double? Accuracy,
    double? Bearing,
    double? Speed
    ) : ICommand;
