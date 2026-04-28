using Domain.Address;
using Domain.Restaurant;

namespace Application.Abstractions.Services;

public interface IDeliveryFeeService
{
    Task<decimal> CalculateAsync(Domain.Restaurant.Restaurant restaurant, Address deliveryAddress, CancellationToken cancellationToken = default);
}
