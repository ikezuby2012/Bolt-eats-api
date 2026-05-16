using Application.Abstractions.Services;
using Domain.Rider;
using SharedKernel;

namespace Application.Tracking.PushRiderLocation;

internal sealed class BroadcastLocationDomainEventHandler(ITrackingService trackingService) : IDomainEventHandler<BroadcastLocationDomain>
{
    public async Task Handle(BroadcastLocationDomain domainEvent, CancellationToken cancellationToken)
    {
        await trackingService.BroadcastLocationAsync(domainEvent.OrderId, domainEvent.payload, cancellationToken);
    }
}
