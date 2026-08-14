using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

[Authorize]
public sealed class OrderHub : Hub
{
    public async Task SubscribeToOrder(string orderId) => await Groups.AddToGroupAsync(Context.ConnectionId, OrderGroup(orderId));

    public async Task UnsubscribeFromOrder(string orderId) => await Groups.RemoveFromGroupAsync(Context.ConnectionId, OrderGroup(orderId));

    // ── Owner ───────────────────────────────────────────────────────────── 
    public async Task SubscribeToRestaurant(string restaurantId) => await Groups.AddToGroupAsync(Context.ConnectionId, RestaurantGroup(restaurantId));

    public async Task UnsubscribeFromRestaurant(string restaurantId) =>
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId, RestaurantGroup(restaurantId));

    // ── Rider ─────────────────────────────────────────────────────────────
    public async Task SubscribeAsRider(string riderId) =>
        await Groups.AddToGroupAsync(
            Context.ConnectionId, RiderGroup(riderId));

    public async Task UnsubscribeAsRider(string riderId) =>
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId, RiderGroup(riderId));

    // ── Group key helpers ─────────────────────────────────────────────────
    public static string OrderGroup(string orderId) => $"order:{orderId}";
    public static string OrderGroup(Guid orderId) => $"order:{orderId}";
    public static string RestaurantGroup(string id) => $"restaurant:{id}";
    public static string RestaurantGroup(Guid id) => $"restaurant:{id}";
    public static string RiderGroup(string riderId) => $"rider:{riderId}";
    public static string RiderGroup(Guid riderId) => $"rider:{riderId}";
}
