using Application.Abstractions.Services;
using Domain.Restaurant;

namespace Infrastructure.Services;

internal sealed class DeliveryFeeService : IDeliveryFeeService
{
    private static readonly (double MaxKm, decimal Fee)[] Brackets =
    [
        (2.0,  0.00m),
        (5.0,  1.50m),
        (10.0, 3.00m),
        (15.0, 5.00m),
        (double.MaxValue, 8.00m)
    ];

    public Task<decimal> CalculateAsync(Restaurant restaurant, Domain.Address.Address deliveryAddress, CancellationToken cancellationToken = default)
    {
        decimal baseFee = restaurant.DeliveryFeeMin ?? 0m;
        double? distanceMetres = restaurant.Addresses?.FirstOrDefault()?.Location?.Distance(deliveryAddress.Location);
        double? distanceKm = distanceMetres / 1000.0;
        (double MaxKm, decimal Fee) bracket = Brackets.First(b => distanceKm <= b.MaxKm);
        decimal fee = Math.Max(baseFee, bracket.Fee);

        if (restaurant.DeliveryFeeMax.HasValue)
        {
            fee = Math.Min(fee, restaurant.DeliveryFeeMax.Value);
        }

        return Task.FromResult(fee);
    }
}
