using Domain.Payment;

namespace Application.Abstractions.Services.Payments;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(int GatewayId);
    IPaymentGateway GetDefault();
}
