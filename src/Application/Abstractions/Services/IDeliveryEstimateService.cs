using Application.Tracking.Dto;
using Domain.Address;

namespace Application.Abstractions.Services;

public interface IDeliveryEstimateService
{
    Task<DeliveryEstimate> EstimateAsync(Domain.Restaurant.Restaurant restaurant, Address deliveryAddress, CancellationToken cancellationToken = default);
}
