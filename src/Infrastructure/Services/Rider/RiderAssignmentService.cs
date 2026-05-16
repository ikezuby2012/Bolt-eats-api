using Application.Abstractions.Data;
using Application.Abstractions.Services.Rider;
using Application.Tracking.Dto;
using Domain.Order;
using GoogleApi;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Common.Enums;
using GoogleApi.Entities.Maps.DistanceMatrix.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Infrastructure.Services.Rider;

internal sealed class RiderAssignmentService(IApplicationDbContext context, IConfiguration config, IDateTimeProvider dateTimeProvider) : IRiderAssignmentService
{
    public async Task<DistanceMatrixResult> GetDurationInTrafficAsync(GeoCoordinateDto origin, GeoCoordinateDto destination, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DistanceMatrixRequest
            {
                Key = config["Google:ApiKey"]!,
                Origins = [new LocationEx(new CoordinateEx(origin.Latitude, origin.Longitude))],
                Destinations = [new LocationEx(new CoordinateEx(destination.Latitude, destination.Longitude))],
                TravelMode = TravelMode.DRIVING,
                DepartureTime = dateTimeProvider.UtcNow,
                TrafficModel = TrafficModel.Best_Guess,
                Units = Units.Metric,
            };

            GoogleApi.Entities.Maps.DistanceMatrix.Response.DistanceMatrixResponse response = await GoogleMaps.DistanceMatrix.QueryAsync(
                request, cancellationToken);

            if (response.Status != Status.Ok)
            {
                return new DistanceMatrixResult(false, 0, 0, $"Google API error: {response.Status}");
            }

            GoogleApi.Entities.Maps.DistanceMatrix.Response.Element? element = response.Rows?.FirstOrDefault()?.Elements?.FirstOrDefault();

            if (element?.Status != Status.Ok)
            {
                return new DistanceMatrixResult(false, 0, 0, $"Route element error: {element?.Status}");
            }

            int durationSeconds = element.DurationInTraffic?.Value ?? element.Duration?.Value ?? 0;

            int distanceMetres = element.Distance?.Value ?? 0;

            return new DistanceMatrixResult(
                IsSuccess: true,
                DurationInTrafficSeconds: durationSeconds,
                DistanceMetres: distanceMetres);
        }
        catch (Exception ex)
        {
            return new DistanceMatrixResult(false, 0, 0, ex.Message);
        }
    }

    public async Task TryAutoAssignAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        Order? order = await context.Order
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            return;
        }

        Domain.Users.User? availableRider = await context.Users
            .Where(u =>
                u.RoleId == Domain.Users.UserRole.Rider.Id &&
                u.IsOnline &&
                !context.Order.Any(o =>
                    o.RiderId == u.Id &&
                    (o.OrderStatusId == EOrderStatus.Cancelled.Id ||
                     o.OrderStatusId == EOrderStatus.ReadyForPickup.Id)))
            //.OrderBy(u =>
            //    u.Addresses.First().L.Distance(order.DeliveryAddress.Location))
            .FirstOrDefaultAsync(cancellationToken);

        if (availableRider is null)
        {
            return;
        }

        order.RiderId = availableRider.Id;
        await context.SaveChangesAsync(cancellationToken);
    }
}
