using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Rider;
using Application.Tracking.Dto;
using Domain.Order;
using Domain.Rider;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tracking.GetOrderTracking;

internal sealed class GetOrderTrackingQueryHandler(IApplicationDbContext db, IRiderLocationCache locationCache) : IQueryHandler<GetOrderTrackingQuery, OrderTrackingDto>
{
    public async Task<Result<OrderTrackingDto>> Handle(GetOrderTrackingQuery query, CancellationToken cancellationToken)
    {
        Domain.Order.Order? order = await db.Order
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Restaurant)
                .ThenInclude(r => r.Addresses)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(
                o => o.Id == query.OrderId,
                cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderTrackingDto>(
                Error.NotFound("Order.NotFound", "Order not found."));
        }

        bool isCustomer = order.CustomerId == query.UserId;
        bool isRider = order.RiderId == query.UserId;

        User? user = await db.Users
           .AsNoTracking()
           .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        bool isOwner = user?.RoleId == UserRole.BusinessOwner.Id &&
                       await db.Restaurants.AnyAsync(
                           r => r.Id == order.RestaurantId &&
                                r.OwnerId == query.UserId,
                           cancellationToken);

        bool isAdmin = user?.RoleId == UserRole.Admin.Id;

        if (!isCustomer && !isRider && !isOwner && !isAdmin)
        {
            return Result.Failure<OrderTrackingDto>(
                Error.NotFound("Order.NotFound", "Order not found."));
        }

        EOrderStatus currentStatus = EOrderStatus.FromValue(order.OrderStatusId)!;
        int progressStep = ResolveProgressStep(currentStatus);
        (string arrivalTime, string latestArrivalTime) = ComputeEta(order);

        double? riderLat = null;
        double? riderLng = null;
        double? riderHeading = null;
        RiderProfile? riderProfile = null;

        if (order.RiderId.HasValue)
        {
            RiderMeta? meta = await locationCache.GetMetaAsync(
                order.RiderId.Value, cancellationToken);


            IReadOnlyList<NearbyRider> nearbyRiders = await locationCache.GetNearbyRidersAsync(
                (double)order.Address.Latitude!.Value,
                (double)order.Address.Longitude!.Value,
                radiusKm: 50,
                maxResults: 100,
                cancellationToken: cancellationToken);

            NearbyRider? assignedRider = nearbyRiders
                .FirstOrDefault(r => r.RiderId == order.RiderId.Value);

            if (assignedRider is not null)
            {
                riderLat = assignedRider.Latitude;
                riderLng = assignedRider.Longitude;
                riderHeading = meta?.Heading;
            }

            riderProfile = await db.RiderProfiles.AsNoTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == order.RiderId.Value, cancellationToken);
        }

        Domain.Address.Address? restaurantAddress = order.Restaurant.Addresses.FirstOrDefault();
        string restaurantLabel = restaurantAddress is not null
            ? $"{order.Restaurant.Name}, {restaurantAddress.Street}"
            : order.Restaurant.Name;

        string deliveryAddress = $"{order.Address.Street}, {order.Address.City}, " + $"{order.Address.State}, {order.Address.Country}";

        string riderFullName = riderProfile?.User is not null
            ? $"{riderProfile.User.FirstName} {riderProfile.User.LastName}".Trim()
            : string.Empty;

        var dto = new OrderTrackingDto(
            OrderId: order.Id,
            Status: currentStatus.Name,
            StatusLabel: ResolveStatusLabel(currentStatus),
            ProgressStep: progressStep,
            ArrivalTime: arrivalTime,
            LatestArrivalTime: latestArrivalTime,
            RestaurantName: restaurantLabel,
            RestaurantAddress: restaurantAddress?.Street ?? string.Empty,
            RiderLatitude: riderLat,
            RiderLongitude: riderLng,
            RiderHeading: riderHeading,
            riderName: riderFullName,
            riderPlate: riderProfile?.NumberPlate ?? string.Empty,
            riderVehicle: riderProfile is not null
                ? $"{riderProfile.VehicleType}, {riderProfile.VehicleMake}, {riderProfile.VehicleModel}, color: {riderProfile.VehicleColor}"
                : string.Empty,
            riderAvatarImg: riderProfile?.VehiclePhotoUrl ?? string.Empty,
            riderRating: 4.5,
            RiderImg: riderProfile?.User.ProfileImageUrl ?? string.Empty,
            DeliveryLatitude: (double)order.Address.Latitude!.Value,
            DeliveryLongitude: (double)order.Address.Longitude!.Value,
            DeliveryAddress: deliveryAddress,
            DeliveryType: "Standard",
            Instructions: order.Notes,
            ServiceType: "Standard",
            Total: order.Total,
            Items: order.Items
                .Select(i => new TrackingOrderItemDto(
                    i.Name, i.Quantity, i.UnitPrice))
                .ToList());

        return Result.Success(dto);
    }

    private static string ResolveStatusLabel(EOrderStatus status) =>
    status.Name switch
    {
        "Pending" => "Order Placed",
        "Accepted" => "Order Confirmed",
        "Preparing" => "Preparing your order",
        "Ready_For_Pickup" => "Rider picking up",
        "In_Transit" => "On the way",
        "Delivered" => "Delivered",
        "Cancelled" => "Cancelled",
        _ => string.Empty
    };

    private static int ResolveProgressStep(EOrderStatus status)
    {
        if (status == EOrderStatus.Pending)
        { return 0; }

        if (status == EOrderStatus.Accepted)
        { return 0; }
        if (status == EOrderStatus.Preparing)
        { return 1; }
        if (status == EOrderStatus.ReadyForPickup)
        { return 2; }
        if (status == EOrderStatus.InTransit)
        { return 3; }
        if (status == EOrderStatus.Delivered)
        { return 4; }
        return 0;
    }

    private static (string arrival, string latest) ComputeEta(Domain.Order.Order order)
    {
        // Use stored estimate from when order was placed
        int estimatedMinutes = order.EstimatedDeliveryMinutes ?? 30;
        int bufferMinutes = 15;   // latest = estimate + 15 min buffer

        // Base time — use AcceptedAt if available, else CreatedAt
        DateTime baseTime = order.AcceptedAt != null ? order.AcceptedAt.Value : order.CreatedAt!.Value;

        DateTime arrival = baseTime.AddMinutes(estimatedMinutes);
        DateTime latest = arrival.AddMinutes(bufferMinutes);

        // Convert to WAT (UTC+1) for Nigerian users
        var wat = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");

        DateTime arrivalWat = TimeZoneInfo.ConvertTimeFromUtc(arrival, wat);
        DateTime latestWat = TimeZoneInfo.ConvertTimeFromUtc(latest, wat);

        return (
            arrivalWat.ToString("HH:mm", CultureInfo.InvariantCulture),
            latestWat.ToString("HH:mm", CultureInfo.InvariantCulture));
    }
}
