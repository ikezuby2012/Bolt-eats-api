namespace Application.Abstractions.Services.Payments;

public interface IPaymentHubService
{
    Task NotifyPaymentSucceededAsync(
        Guid userId,
        Guid paymentId,
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken = default);

    Task NotifyPaymentFailedAsync(
        Guid userId,
        Guid paymentId,
        string reason,
        CancellationToken cancellationToken = default);
}
