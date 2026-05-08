using Application.Abstractions.Services.Payments;
using Domain.Payment;

namespace Infrastructure.Services.Payment;

internal sealed class PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways) : IPaymentGatewayFactory
{
    private readonly Dictionary<int, IPaymentGateway> _map = gateways.ToDictionary(g => g.Gateway);

    public IPaymentGateway GetDefault() => _map.TryGetValue(PaymentGateway.Monnify.Id, out IPaymentGateway? gateway) ? gateway : _map.Values.First();

    public IPaymentGateway GetGateway(int GatewayId)
    {
        if (!_map.TryGetValue(GatewayId, out IPaymentGateway? gateway))
        {
            throw new NotSupportedException($"Payment gateway with Id '{GatewayId}' is not supported.");
        }

        return gateway;
    }
}
