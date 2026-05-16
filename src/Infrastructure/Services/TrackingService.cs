using Application.Abstractions.Services;
using Application.Tracking.Dto;
using Domain.Rider;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services;

internal sealed class TrackingService(IHubContext<TrackingHub> hubContext) : ITrackingService
{
    private static string OrderGroup(Guid orderId) => $"order:{orderId}";

    public Task BroadcastLocationAsync(Guid orderId, RiderLocationUpdatedPayload payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(OrderGroup(orderId)).SendAsync("RiderLocationUpdated", payload, cancellationToken);

    public Task BroadcastStatusChangeAsync(Guid orderId, OrderStatusChangedPayload payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(OrderGroup(orderId)).SendAsync("OrderStatusChanged", payload, cancellationToken);
}
