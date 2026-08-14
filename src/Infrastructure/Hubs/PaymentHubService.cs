using Application.Abstractions.Services.Payments;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

public sealed class PaymentHubService(IHubContext<PaymentHub> hub) : IPaymentHubService
{
    public Task NotifyPaymentSucceededAsync(
        Guid userId,
        Guid paymentId,
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken = default) =>
        hub.Clients
            .Group(PaymentHub.PaymentGroup(paymentId))
            .SendAsync(
                "PaymentSucceeded",
                new
                {
                    paymentId = paymentId.ToString(),
                    orderId = orderId.ToString(),
                    amount,
                    status = "Succeeded"
                },
                cancellationToken);

    public Task NotifyPaymentFailedAsync(
        Guid userId,
        Guid paymentId,
        string reason,
        CancellationToken cancellationToken = default) =>
        hub.Clients
            .Group(PaymentHub.PaymentGroup(paymentId))
            .SendAsync(
                "PaymentFailed",
                new
                {
                    paymentId = paymentId.ToString(),
                    reason,
                    status = "Failed"
                },
                cancellationToken);
}
