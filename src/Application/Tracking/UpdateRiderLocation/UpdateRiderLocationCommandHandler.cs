using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Rider;
using Application.Tracking.Dto;
using SharedKernel;

namespace Application.Tracking.UpdateRiderLocation;

internal sealed class UpdateRiderLocationCommandHandler(IRiderLocationCache locationCache) : ICommandHandler<UpdateRiderLocationCommand>
{
    public async Task<Result> Handle(UpdateRiderLocationCommand command, CancellationToken cancellationToken)
    {
        await locationCache.UpdateLocationAsync(
            command.RiderId,
            command.Latitude,
            command.Longitude,
            command.Heading,
            command.Speed,
            cancellationToken);

        await locationCache.SetStatusAsync(
            command.RiderId,
            RiderAvailability.Available,
            cancellationToken);

        return Result.Success();
    }
}
