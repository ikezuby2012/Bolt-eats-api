using System;
using System.Collections.Generic;
using System.Text;
using Application.Abstractions.Services.Order;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;
public sealed class OrderHubService(IHubContext<OrderHub> hub) : IOrderHubService
{
    public Task NotifyOrderStatusChangedAsync(
       Guid orderId,
       Guid customerId,
       Guid restaurantId,
       Guid? riderId,
       string newStatus,
       string statusLabel,
       DateTime updatedAt,
       CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            orderId = orderId.ToString(),
            newStatus,
            updatedAt = updatedAt.ToString("o"),
            riderId = riderId?.ToString()
        };

        // Push to order-specific group (customer + rider on tracking page)
        return hub.Clients
            .Group(OrderHub.OrderGroup(orderId))
            .SendAsync("OrderStatusChanged", payload, cancellationToken);
    }

    // Called when a new order is placed — notifies owner dashboard
    public Task NotifyNewOrderAsync(Guid restaurantId, object orderSummary, CancellationToken cancellationToken = default) =>
        hub.Clients
            .Group(OrderHub.RestaurantGroup(restaurantId))
            .SendAsync("NewOrderReceived", orderSummary, cancellationToken);

    public Task NotifyRiderAssignedAsync(Guid riderId, object orderSummary, CancellationToken cancellationToken = default) =>
        hub.Clients
            .Group(OrderHub.RiderGroup(riderId))
            .SendAsync("OrderAssigned", orderSummary, cancellationToken);
}
