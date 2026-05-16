using Application.Abstractions.Data;
using Application.Tracking.Dto;
using Application.Users.Dto;
using Domain.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Hubs;

[Authorize]
public sealed class TrackingHub(IApplicationDbContext db) : Hub
{
    private static string OrderGroup(Guid orderId) => $"order:{orderId}";

    /// <summary>
    /// Flutter client calls this after connecting.
    /// Adds the connection to the SignalR group for the given order.
    /// </summary>
    public async Task SubscribeOrder(Guid orderId)
    {
        Guid userId = GetUserId();

        // Verify the caller is authorised to track this order
        bool canTrack = await CanTrackOrderAsync(userId, orderId);
        if (!canTrack)
        {
            await Clients.Caller.SendAsync("Error", new
            {
                Code = "UNAUTHORISED",
                Message = "You are not authorised to track this order."
            });
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, OrderGroup(orderId));

        // Send current snapshot immediately so Flutter doesn't wait for next ping
        OrderTrackingSnapshotDto? snapshot = await GetSnapshotAsync(orderId);
        if (snapshot is not null)
        {
            await Clients.Caller.SendAsync("TrackingSnapshot", snapshot);
        }

    }

    /// <summary>
    /// Flutter client calls this when navigating away from TrackingPage.
    /// </summary>
    public async Task UnsubscribeOrder(Guid orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, OrderGroup(orderId));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Groups are cleaned up automatically by SignalR on disconnect —
        // no manual removal needed here
        await base.OnDisconnectedAsync(exception);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Guid GetUserId()
    {
        string? claim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out Guid id) ? id : Guid.Empty;
    }

    private async Task<bool> CanTrackOrderAsync(Guid userId, Guid orderId)
    {
        // Customer tracking their own order
        bool isOwner = await db.Order
            .AnyAsync(o => o.Id == orderId && o.CustomerId == userId);

        if (isOwner)
        {
            return true;
        }


        // Rider assigned to this order
        bool isRider = await db.Order
            .AnyAsync(o => o.Id == orderId && o.RiderId == userId);

        if (isRider)
        {
            return true;
        }


        // Admin — check role claim
        bool isAdmin = Context.User?.IsInRole("Admin") ?? false;
        return isAdmin;
    }

    private async Task<OrderTrackingSnapshotDto?> GetSnapshotAsync(Guid orderId)
    {
        Domain.Order.Order? order = await db.Order
            .AsNoTracking()
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
        {
            return null;
        }

        Domain.Rider.RiderLocation? location = await db.RiderLocations
            .AsNoTracking()
            .Where(r => r.OrderId == orderId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        return new OrderTrackingSnapshotDto(
            OrderId: order.Id,
            Status: EOrderStatus.FromValue(order.OrderStatusId)!.Name ?? "",
            RiderLocation: (RiderLocationDto)location!,
            DeliveryAddress: (AddressDto)order.Address,
            EstimatedMinutesRemaining: order.EstimatedDeliveryMinutes);
    }
}
