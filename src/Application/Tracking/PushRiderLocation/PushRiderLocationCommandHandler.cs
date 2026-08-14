using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Rider;
using Domain.Order;
using Domain.Rider;
using Microsoft.EntityFrameworkCore;
using SharedKernel;


namespace Application.Tracking.PushRiderLocation;

internal sealed class PushRiderLocationCommandHandler(IApplicationDbContext db, IRiderLocationCache locationCache) : ICommandHandler<PushRiderLocationCommand>
{
    private const double MinAccuracyMetres = 50.0;
    public async Task<Result> Handle(PushRiderLocationCommand command, CancellationToken cancellationToken)
    {
        Order? order = await db.Order.FirstOrDefaultAsync(
            o => o.Id == command.OrderId &&
                 o.RiderId == command.RiderId &&
                 (o.OrderStatusId == EOrderStatus.ReadyForPickup.Id ||
                  o.OrderStatusId == EOrderStatus.InTransit.Id),
            cancellationToken);

        if (order is null)
        {
            return Result.Failure(Domain.Common.CommonErrors.CustomErrorMessage("No active order found for this rider!"));
        }

        if (command.Accuracy.HasValue && command.Accuracy > MinAccuracyMetres)
        {
            return Result.Success();
        }

        var newRiderLocation = new RiderLocation
        {
            Id = Guid.NewGuid(),
            RiderId = command.RiderId,
            OrderId = command.OrderId,
            Latitude = (decimal)command.Latitude,
            Longitude = (decimal)command.Longitude,
            Accuracy = command.Accuracy,
            Bearing = command.Bearing ?? 0.0,
            Speed = command.Speed ?? 0.0
        };

        db.RiderLocations.Add(newRiderLocation);

        await locationCache.UpdateLocationAsync(
            command.RiderId,
            command.Latitude,
            command.Longitude,
            command.Bearing,
            command.Speed,
            cancellationToken);

        newRiderLocation.Raise(new BroadcastLocationDomain(command.OrderId, new RiderLocationUpdatedPayload(command.RiderId, command.OrderId,
                command.Latitude, command.Longitude,
                command.Bearing, command.Speed,
                DateTime.UtcNow)));

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
