using SharedKernel;

namespace Domain.Rider;

public sealed record BroadcastLocationDomain(Guid OrderId, RiderLocationUpdatedPayload payload) : IDomainEvent;
