using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Abstractions.Services.Rider;
using Application.Tracking.Dto;
using Domain.Order;
using Domain.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

internal sealed class DeliveryEstimateService(IApplicationDbContext db, IRiderAssignmentService distanceMatrix) : IDeliveryEstimateService
{
    private const int BufferMinutes = 5;
    //private const int FallbackTravelMinutes = 20;

    /// <summary>
    /// TODO IMPROVEMENT IN THE FUTURE
    /// ETA = nearest rider trave + kitchen prep + delivery route
    /// </summary>
    /// <param name="restaurant"></param>
    /// <param name="deliveryAddress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<DeliveryEstimate> EstimateAsync(Restaurant restaurant, Domain.Address.Address deliveryAddress, CancellationToken cancellationToken = default)
    {
        int activePrepOrders = await db.Order
            .CountAsync(o => o.RestaurantId == restaurant.Id && (o.OrderStatusId == EOrderStatus.Accepted.Id || o.OrderStatusId == EOrderStatus.Preparing.Id), cancellationToken);

        int basePrepMinutes = restaurant.EstDeliveryMin ?? 15;
        int prepPressureMinutes = activePrepOrders / 5 * 2;
        int prepMinutes = basePrepMinutes + prepPressureMinutes;

        Domain.Address.Address? restaurantAddress = restaurant.Addresses.FirstOrDefault();

        if (restaurantAddress is null)
        {
            return new DeliveryEstimate(0, 0, 0, 0, 0, false);
        }

        var origin = new GeoCoordinateDto(restaurantAddress.Location!.Y, restaurantAddress.Location!.X);
        var destination = new GeoCoordinateDto(deliveryAddress.Location!.Y, deliveryAddress.Location!.X);

        DistanceMatrixResult matrixResult = await distanceMatrix.GetDurationInTrafficAsync(origin, destination, cancellationToken);
        int travelMinutes;
        int distanceMetres;
        bool isTrafficBased;

        if (matrixResult.IsSuccess)
        {
            travelMinutes = (int)Math.Ceiling(matrixResult.DurationInTrafficSeconds / 60.0);
            distanceMetres = matrixResult.DistanceMetres;
            isTrafficBased = true;
        }
        else
        {
            // Google API failed — fall back to straight-line approximation
            double straightLineMetres = restaurantAddress.Location.Distance(deliveryAddress.Location);

            travelMinutes = (int)Math.Ceiling(straightLineMetres / 1000.0 * 1.3 / 25.0 * 60.0);
            distanceMetres = (int)straightLineMetres;
            isTrafficBased = false;
        }
        int total = prepMinutes + travelMinutes + BufferMinutes;

        if (restaurant.EstDeliveryMin.HasValue && restaurant.EstDeliveryMax.HasValue)
        {
            total = Math.Clamp(
                total,
                restaurant.EstDeliveryMin.Value,
                restaurant.EstDeliveryMax.Value);
        }


        return new DeliveryEstimate(
            TotalMinutes: total,
            PrepMinutes: prepMinutes,
            TravelMinutes: travelMinutes,
            BufferMinutes: BufferMinutes,
            DistanceMetres: distanceMetres,
            IsTrafficBased: isTrafficBased);
    }
}
